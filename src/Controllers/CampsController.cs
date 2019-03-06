using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetAll(bool includeTalks = false)
        {
            try
            {
                var camps = await repository.GetAllCampsAsync(includeTalks);

                return mapper.Map<CampModel[]>(camps);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }
        }
        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDte(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var camps = await repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!camps.Any()) return NotFound();

                return mapper.Map<CampModel[]>(camps);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }
        }
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);

                if (camp == null) return NotFound();

                return mapper.Map<CampModel>(camp);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }
        }
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var camp = await repository.GetCampAsync(model.Moniker);

                if (camp != null)
                {
                    return BadRequest("Moniker already exist");
                }

                var uri = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });


                if (string.IsNullOrWhiteSpace(uri))
                {
                    return BadRequest();
                }

                var newCamp = mapper.Map<Camp>(model);

                this.repository.Add(newCamp);

                if (await repository.SaveChangesAsync())
                {
                    return Created(uri, mapper.Map<CampModel>(newCamp));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }

            return BadRequest();
        }
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Post(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await repository.GetCampAsync(moniker);

                if (oldCamp == null) return NotFound();

                mapper.Map(model, oldCamp);

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(oldCamp);
                }

            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);

                if (camp == null) return NotFound();

                repository.Delete(camp);

                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }

            return BadRequest();
        }
    }
}
