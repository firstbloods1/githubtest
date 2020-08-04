

using Microsoft.AspNetCore.Mvc.Filters;

namespace DXL_MVC.SQL
{
    public class AntiSqlInjectAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //全局处理sql访问action时的参数，防止注入漏洞攻击
            var actionParameters = context.ActionArguments; foreach (var p in actionParameters)
            {
                if (p.GetType() == typeof(string)) 
                { 
                    actionParameters[p.Key] = StringHelper.FilterSql(p.Value.ToString()); 
                }
            }
        }
    }
}
