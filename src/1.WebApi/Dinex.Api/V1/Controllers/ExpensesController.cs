//namespace Dinex.Api.V1.Controllers
//{
//    [ApiVersion("1.0")]
//    [Route("v{version:apiVersion}/[controller]")]
//    public class ExpensesController : MainController
//    {
//        private readonly IMediator _mediator;

//        public ExpensesController(IMediator mediator)
//        {
//            _mediator = mediator;
//        }

//        [Authorize]
//        [HttpPost("register")]
//        public async Task<IActionResult> RegisterExpense([FromBody] RegisterExpenseCommand command)
//        {
//            var result = await _mediator.Send(command);
//            return Ok(result);
//        }

//        [Authorize]
//        [HttpGet("list")]
//        public async Task<IActionResult> ListExpenses()
//        {
//            var result = await _mediator.Send(new ListExpensesQuery());
//            return Ok(result);
//        }
//    }
//}
