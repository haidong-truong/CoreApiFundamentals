using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> ListTalksByMonikerAsync(string moniker)
        {
            try
            {
                var talks = await repository.GetTalksByMonikerAsync(moniker, true);

                if (talks == null) return NotFound();

                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetTalkByMonikerAsync(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null) return NotFound();

                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> CreateAsync(string moniker, TalkModel model)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);

                if (camp == null) return BadRequest("Camp is not found");

                var talk = mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speak is required");

                var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);

                if (speaker == null) return BadRequest("Speak could not be found.");

                talk.Speaker = speaker;

                repository.Add(talk);

                if (await repository.SaveChangesAsync())
                {
                    var uri = linkGenerator.GetPathByAction(HttpContext, "GetTalkByMonikerAsync", values: new { moniker, id = talk.TalkId });

                    return Created(uri, mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception)
            {
                this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }

            return BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> UpdateAsync(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null) return BadRequest("Talk is not found.");

                mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    talk.Speaker = speaker;
                }

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Updated talk failed.");
                }
            }
            catch (Exception)
            {
                this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }

            return BadRequest();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id);

                if (talk == null) return BadRequest("Cannot find the talk to delete.");

                repository.Delete(talk);

                if (await repository.SaveChangesAsync())
                    return Ok();
                else
                    return BadRequest("Failed to delete talk.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failed.");
            }
        }
    }
}
