using EcommerceApp.Api.Models;
using EcommerceApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Api.Controllers;
/// <summary>
/// Controlador para procesar operaciones del carrito.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CartController
{
  /// <summary>
  /// Servicio de carrito.
  /// </summary>
  private readonly ICartService _cartService;

  /// <summary>
  /// Servicio de pago.
  /// </summary>
  private readonly IPaymentService _paymentService;

  /// <summary>
  /// Servicio de envío.
  /// </summary>
  private readonly IShipmentService _shipmentService;

  /// <summary>
  /// Servicio de descuento.
  /// </summary>
  private readonly IDiscountService _discountService;

  /// <summary>
  /// Constructor del CartController.
  /// </summary>
  public CartController(
      ICartService cartService,
      IPaymentService paymentService,
      IShipmentService shipmentService,
      IDiscountService discountService
  )
  {
    _cartService = cartService;
    _paymentService = paymentService;
    _shipmentService = shipmentService;
    _discountService = discountService;
  }

  /// <summary>
  /// Ejecuta el proceso de checkout con pago y envío.
  /// </summary>
  /// <param name="card">Datos de la tarjeta</param>
  /// <param name="addressInfo">Datos de dirección</param>
  /// <returns>Resultado del proceso</returns>
  [HttpPost]
  public string CheckOut(ICard card, IAddressInfo addressInfo)
  {
    var total = _cartService.Total();
    var discountedTotal = _discountService.ApplyDiscount(total);

    var result = _paymentService.Charge(discountedTotal, card);
    if (result)
    {
      _shipmentService.Ship(addressInfo, _cartService.Items());
      return "charged";
    }
    else
    {
      return "not charged";
    }
  }
}