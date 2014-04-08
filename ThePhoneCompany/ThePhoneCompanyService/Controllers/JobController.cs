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
    public class JobController : TableController<Job>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            ThePhoneCompanyBackendContext context = new ThePhoneCompanyBackendContext();
            DomainManager = new EntityDomainManager<Job>(context, Request, Services);
        }

        public IQueryable<Job> GetAllJobs()
        {
            return Query();
        }

        public SingleResult<Job> GetJob(string id)
        {
            return Lookup(id);
        }

        public Task<Job> PatchTodoItem(string id, Delta<Job> patch)
        {
            return UpdateAsync(id, patch);
        }

        public async Task<IHttpActionResult> PostTodoItem(Job item)
        {
            Job current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}