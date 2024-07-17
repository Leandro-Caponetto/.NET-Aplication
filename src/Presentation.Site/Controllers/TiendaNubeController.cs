using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.V1.TiendaNube.LoginCA;
using Core.V1.TiendaNube.RegisterTienda;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Core.Exceptions;
using Core.V1.TiendaNube.ShippingCarrier;
using Core.V1.TiendaNube.FormRegister;
using Core.V1.TiendaNube.DocumentTypeList;
using Core.V1.TiendaNube.GetBranchOffices;
using Core.V1.TiendaNube.AddNewOrders;
using System.Collections;
using Microsoft.Extensions.Primitives;

namespace Presentation.Site.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("")]
    [Route("/[controller]/")]
    public class TiendaNubeController : Controller
    {

        private readonly IMediator mediator;

        public TiendaNubeController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));

        }

        public ActionResult Index()
        {

           
            return View();
        }

        [HttpGet("AutorizeNewTienda")]
        public async Task<IActionResult> AuthorizeNewUserAsync([FromQuery] RegisterTiendaRequest request)
        {
            try
            {
                string sreferer = "/admin/apps";
                {
                    StringValues referer;
                    HttpContext.Request.Headers.TryGetValue("Referer", out referer);

                    int len = sreferer.Length;

                    string aux = referer;
                    if (aux.Length < len + 2 || !aux.Substring(aux.Length - len).Equals(sreferer) && !aux.Substring(aux.Length - len - 1).Equals(sreferer + "/"))
                        sreferer = aux + (aux.EndsWith('/') ? "" : "/") + sreferer.Substring(1) + "/";
                }

                HttpContext.Session.SetString("code", request.Code);
                HttpContext.Session.SetString("referer", sreferer);

                //guardar en variable de session el code
                await mediator.Send(request);

                ViewBag.Error = false;
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpPost("LoginUserCA")]
        public async Task<IActionResult> LoginCA(LoginCARequest request)
        {

            try
            {
                var code = HttpContext.Session.GetString("code");
                if (string.IsNullOrEmpty(code))
                    throw new BusinessException("No existe Codigo de referencia");

                string referer = HttpContext.Session.GetString("referer");

                request.Code = code;

                var responseLogin = await mediator.Send(request);

                if (string.IsNullOrEmpty(responseLogin))
                {
                    var shippingRequest = new ShippingCarrierRequest()
                    {
                        Code = code
                    };

                    await mediator.Send(shippingRequest);

                    ViewBag.Error = true;
                    ViewBag.ErrorMessage = referer;
                    return View("Exito");
                }
                else
                {
                    ViewBag.Error = true;
                    ViewBag.ErrorMessage = responseLogin;
                    return View("Login");
                }
            }
            catch (Exception)
            {
                ViewBag.Error = true;
                return View("Error");
            }


        }


        [HttpGet("RegisterUserCA")]
        public async Task<IActionResult> RegisterUserCA()
        {

            var request = new DocumentTypeListRequest();

            ViewBag.DocumentTypes = await mediator.Send(request);
            ViewBag.Error = false;
            return View("Registro");

        }

        [HttpPost("RegisterProcess")]
        public async Task<IActionResult> RegisterProcess(FormRegisterRequest request)
        {
            try
            {
                var code = HttpContext.Session.GetString("code");
                string referer = HttpContext.Session.GetString("referer");

                if (string.IsNullOrEmpty(code))
                    throw new BusinessException("No existe Codigo de referencia");

                request.Code = code;

                var responseRegister = await mediator.Send(request);
                if (string.IsNullOrEmpty(responseRegister))
                {
                    var shippingRequest = new ShippingCarrierRequest()
                    {
                        Code = code
                    };

                    await mediator.Send(shippingRequest);

                    ViewBag.Error = true;
                    ViewBag.ErrorMessage = referer;

                    return View("Exito");
                }
                var requestDocType = new DocumentTypeListRequest();

                ViewBag.DocumentTypes = await mediator.Send(requestDocType);

                ViewBag.Error = true;
                ViewBag.ErrorMessage = responseRegister;
                return View("Registro");
            }
            catch (Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }


        [HttpGet("AddNewOrders")]
        public IActionResult AddNewOrders()
        {
            try
            {
                var srvprv = HttpContext.RequestServices;

                var test = HttpContext.Request.Query;
                /*
                if(test.ContainsKey("id[]") && test.ContainsKey("store"))
                {
                    IEnumerable<string> ids = test["id[]"];
                    var request = new AddNewOrdersRequest()
                    {
                        id = ids,
                        store = Int32.Parse(test["store"])
                    };
                    // await mediator.Send(request);
                    return View("Envios");
                }
                */

                if ((test.ContainsKey("id[]") || test.ContainsKey("id")) && test.ContainsKey("store"))
                {
                    IEnumerable<string> ids = test.ContainsKey("id[]") ? test["id[]"] : test["id"];

                    var request = new AddNewOrdersRequest()
                    {
                        id = ids,
                        store = Int32.Parse(test["store"])
                    };
                    // await mediator.Send(request);
                    return View("Envios");
                }

                return View("ErrorCustom");
            }
            catch (Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                return View("ErrorCustom");
            }
        }



        [HttpGet("AddNewOrdersProcNext")]
        public async Task<OrderStatus> AddNewOrdersProcNext()
        {
            OrderStatus resp = null;
            try
            {
                var srvprv = HttpContext.RequestServices;

                var query = HttpContext.Request.Query;
                if (query.ContainsKey("store") && query.ContainsKey("id"))
                {
                    var request = new AddNewOrderProcNextRequest()
                    {
                        storeId = Int32.Parse(query["store"]),
                        orderId = Int32.Parse(query["id"])
                    };

                    resp = await mediator.Send(request);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                //resp = new OrderStatus("Error", true, ex.Message);
            }
            return resp;
        }
    }
}
