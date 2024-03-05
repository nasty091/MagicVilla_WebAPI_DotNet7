using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaNumberAPI")] //[Route("api/[controller]")]
    [ApiController] // Don't have this data annotation, validation in model will not be active
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly IVillaNumberRepository _dbVillaNumber;

        private readonly IMapper _mapper;

        protected APIResponse _response;
        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IMapper mapper)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync();

                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);

                _response.StatusCode = System.Net.HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVillaNumber")] // The number must be integer, "GetVillaNumber" is a route name, {id:int} used to distinguish between GetAll and Get one recode
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type = typeof(VillaDTO))]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(400)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);

                _response.StatusCode = System.Net.HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost(Name = "CreateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                //Use this code instead of [ApiController]
                //if(ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("", "Vill Number already Exists!");
                    return BadRequest(ModelState);
                }

                if (createDTO == null || createDTO.VillaNo == 0)
                {
                    return BadRequest(createDTO);
                }

                //if (villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}

                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);

                //Villa model = new()
                //{
                //    Amenity = villaDTO.Amenity,
                //    Details = villaDTO.Details,
                //    ImageUrl = villaDTO.ImageUrl,
                //    Name = villaDTO.Name,
                //    Occupancy = villaDTO.Occupancy,
                //    Rate = villaDTO.Rate, 
                //    Sqft = villaDTO.Sqft,
                //};

                await _dbVillaNumber.CreateAsync(villaNumber);

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);

                _response.StatusCode = System.Net.HttpStatusCode.Created;

                return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response); // Create Location in Response Headers
                                                                                     //return Ok(villaDTO);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { ex.ToString() };
            }
            return _response;
        }


        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {
                    return NotFound();
                }

                await _dbVillaNumber.RemoveAsync(villaNumber);

                _response.StatusCode = System.Net.HttpStatusCode.NoContent;

                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { ex.ToString() };
            }
            return _response;
        }


        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.VillaNo)
                {
                    return BadRequest();
                }

                //var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
                //villa.Name = villaDTO.Name;
                //villa.Occupancy = villaDTO.Occupancy;
                //villa.Sqft = villaDTO.Sqft;

                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);

                //Villa model = new ()
                //{
                //    Id = villaDTO.Id,
                //    Amenity = villaDTO.Amenity,
                //    Details = villaDTO.Details,
                //    ImageUrl = villaDTO.ImageUrl,
                //    Name = villaDTO.Name,
                //    Occupancy = villaDTO.Occupancy,
                //    Rate = villaDTO.Rate,
                //    Sqft = villaDTO.Sqft,
                //};

                await _dbVillaNumber.UpdateAsync(model);

                _response.StatusCode = System.Net.HttpStatusCode.NoContent;

                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { ex.ToString() };
            }
            return _response;
        }



        [HttpPatch("{id:int}", Name = "UpdatePartialVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVillaNumber(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id, tracked: false); //Use AsNoTracking() because EF Core can not track 2 object at the same time (villa and model)

            VillaNumberUpdateDTO villaNumberDTO = _mapper.Map<VillaNumberUpdateDTO>(villaNumber);

            //VillaUpdateDTO villaDTO = new()
            //{
            //    Id = villa.Id,
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft,
            //};

            if (villaNumber == null)
            {
                return NotFound();
            }

            patchDTO.ApplyTo(villaNumberDTO, ModelState);

            VillaNumber model = _mapper.Map<VillaNumber>(villaNumberDTO);

            //Villa model = new()
            //{
            //    Id = villaDTO.Id,
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft,
            //};

            await _dbVillaNumber.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return NoContent();
        }
    }
}
