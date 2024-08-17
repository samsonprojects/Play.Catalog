using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
using Play.Common.Service.Repositories;
using static Play.Catalog.Contracts.Contracts;

namespace Play.Catalog.Service.Controllers
{

    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private const string AdminRole = "Admin";
        private readonly IRepository<Item> _itemsRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this._itemsRepository = itemsRepository;
            this._publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {


            var items = (await _itemsRepository.GetAllAsync()).Select(item => item.AsDto());

            return Ok(items);

        }

        [HttpGet("{id}")]
        [Authorize(Policies.Read)]
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
        [Authorize(Policies.Write)]
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

            await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);

        }


        /// PUT / items/{id}
        [HttpPut("{id}")]
        [Authorize(Policies.Write)]
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

            await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));


            return NoContent();

        }

        //DELETE /items/{id}

        [HttpDelete]
        [Authorize(Policies.Write)]

        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existingItem = await _itemsRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            await _itemsRepository.RemoveAsync(existingItem.Id);

            await _publishEndpoint.Publish(new CatalogItemDeleted(existingItem.Id));

            return NoContent();

        }


    }
}