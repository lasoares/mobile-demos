using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using ThePhoneCompanyService.DataObjects;
using ThePhoneCompanyService.Models;

namespace ThePhoneCompanyService.Controllers
{
    public class CustomerController : TableController<Customer>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            ThePhoneCompanyBackendContext context = new ThePhoneCompanyBackendContext();
            DomainManager = new EntityDomainManager<Customer>(context, Request, Services);
        }

        public IQueryable<Customer> GetAllCustomers()
        {
            return Query();
        }

        public SingleResult<Customer> GetCustomer(string id)
        {
            return Lookup(id);
        }

        public Task<Customer> PatchTodoItem(string id, Delta<Customer> patch)
        {
            return UpdateAsync(id, patch);
        }

        public async Task<IHttpActionResult> PostTodoItem(Customer item)
        {
            Customer current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}