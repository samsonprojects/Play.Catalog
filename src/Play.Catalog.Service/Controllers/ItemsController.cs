using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
using Play.Common.Service.Repositories;

namespace Play.Catalog.Service.Controllers
{

    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> _itemsRepository;
        private static int requestCounter = 0;

        public ItemsController(IRepository<Item> itemsRepository)
        {
            this._itemsRepository = itemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            // requestCounter++;
            // Console.WriteLine($"Request {requestCounter}: Starting...");

            // if (requestCounter <= 2)
            // {
            //     Console.WriteLine($"Request {requestCounter}: Delaying...");
            //     await Task.Delay(TimeSpan.FromSeconds(10));
            // }

            // if (requestCounter <= 4)
            // {
            //     Console.WriteLine($"Request {requestCounter}: 500 (Internal Server Error)...");
            //     return StatusCode(500);
            // }

            // Console.WriteLine($"Request {requestCounter}: 200 (OK)");
            var items = (await _itemsRepository.GetAllAsync()).Select(item => item.AsDto());
            return Ok(items);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = (await _itemsRepository.GetAsync(id))?.AsDto();
            if (item == null)
            {
                return NotFound();
            }
            return item;

        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> AddItemAsync(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };
            await _itemsRepository.CreateAsync(item);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);

        }


        /// PUT / items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItemAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await _itemsRepository.GetAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }
            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await _itemsRepository.UpdateAsync(existingItem);

            return NoContent();

        }

        //DELETE /items/{id}

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existingItem = await _itemsRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            await _itemsRepository.RemoveAsync(existingItem.Id);
            return NoContent();

        }


    }
}