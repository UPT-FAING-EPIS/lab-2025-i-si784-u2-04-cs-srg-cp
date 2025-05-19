using EcommerceApp.Api.Controllers;
using EcommerceApp.Api.Models;
using EcommerceApp.Api.Services;
using Moq;

namespace EcommerceApp.Tests;

public class ControllerTests
{
    private CartController controller;
    private Mock<IPaymentService> paymentServiceMock;
    private Mock<ICartService> cartServiceMock;

    private Mock<IShipmentService> shipmentServiceMock;
    private Mock<ICard> cardMock;
    private Mock<IAddressInfo> addressInfoMock;
    private List<ICartItem> items;
    private Mock<IDiscountService> discountServiceMock;

    [SetUp]
    public void Setup()
    {
        cartServiceMock = new Mock<ICartService>();
        paymentServiceMock = new Mock<IPaymentService>();
        shipmentServiceMock = new Mock<IShipmentService>();
        discountServiceMock = new Mock<IDiscountService>();

        // arrange
        cardMock = new Mock<ICard>();
        addressInfoMock = new Mock<IAddressInfo>();

        // 
        var cartItemMock = new Mock<ICartItem>();
        cartItemMock.Setup(item => item.Price).Returns(10);

        items = new List<ICartItem>()
          {
              cartItemMock.Object
          };

        cartServiceMock.Setup(c => c.Items()).Returns(items.AsEnumerable());

        // controller = new CartController(cartServiceMock.Object, paymentServiceMock.Object, shipmentServiceMock.Object);
        controller = new CartController(cartServiceMock.Object, paymentServiceMock.Object, shipmentServiceMock.Object, discountServiceMock.Object);
    }

    [Test]
    public void ShouldReturnCharged()
    {
        string expected = "charged";
        paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(true);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());

        Assert.That(expected, Is.EqualTo(result));
    }

    [Test]
    public void ShouldReturnNotCharged()
    {
        string expected = "not charged";
        paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(false);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
        Assert.That(expected, Is.EqualTo(result));
    }

    [TestCase(100, 10, true, "charged")]
    [TestCase(100, 10, false, "not charged")]
    public void CheckOut_WithDiscountService_ReturnsExpectedResult(double total, double discount, bool chargeResult, string expected)
    {
        // arrange
        cartServiceMock.Setup(c => c.Total()).Returns(total);
        discountServiceMock.Setup(d => d.ApplyDiscount(total)).Returns(total - discount);
        paymentServiceMock.Setup(p => p.Charge(total - discount, cardMock.Object)).Returns(chargeResult);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        if (chargeResult)
        {
            shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once);
        }
        else
        {
            shipmentServiceMock.Verify(s => s.Ship(It.IsAny<IAddressInfo>(), It.IsAny<IEnumerable<ICartItem>>()), Times.Never);
        }

        Assert.That(result, Is.EqualTo(expected));
    }
}