using System;
using System.Collections.Generic; //why in here is unnecesary but in the video example it is necesary? 
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")] //https://localhost:5001/items
    [Authorize]
    public class ItemsController : ControllerBase
    {

        //private readonly ItemsRepository itemsRepository = new(); -> change this to use the interface and use the constructure for dependency injection.
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;

        private static int requestCounter = 0; //just for testing.

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) //inject the interface with the items repository dependency.
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        /** Action Results give the capability to return more than 1 type of result.**/
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        //public async Task<IEnumerable<ItemDto>> GetAsync()
        {
            requestCounter++;
            Console.WriteLine($"Request {requestCounter}: Starting...");

            // if (requestCounter <= 2)
            // {
            //     Console.WriteLine($"Request {requestCounter}: Delaying... 10 seconds");
            //     await Task.Delay(TimeSpan.FromSeconds(10));
            // }

            // if (requestCounter <= 4)
            // {
            //     Console.WriteLine($"Request {requestCounter}: 500 (Internal Server Error)");
            //     return StatusCode(500);
            // }

            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());
            Console.WriteLine($"Request {requestCounter}: 200 (OK)");
            return Ok(items);
        }

        //GET /items/{id}}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            //var item = items.Where(item => item.Id == id).SingleOrDefault();
            var item = (await itemsRepository.GetAsync(id));
            if (item == null)
            {
                return NotFound(); //returns 404 if item is not found.
            }
            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            // var item = new ItemDto(Guid.NewGuid(), createItemDto.Name, createItemDto.Description, createItemDto.Price, DateTimeOffset.UtcNow);
            // items.Add(item);
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            //now we can create the item via ItemsRepository.CreateAsync.
            await itemsRepository.CreateAsync(item);

            /*
            After the item is created, publish the event to the message broker:
            */
            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            //for GetByIdAsync to work properly as async, we need to fix the startup.cs (currently .net will remove the asyn suffix at runtime.)
            //startup.cs -> services.AddControllers(options => { options.SuppressAsyncSuffixInActionNames = false;});
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]//to update an item use IActionResult: It will not return anything in the result.
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem);
            /*
            After the item is Updated, publish the event to the message broker:
            */
            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(item.Id);

            /*
            After the item is deleted, publish the event to the message broker:
            */
            await publishEndpoint.Publish(new CatalogItemDeleted(item.Id));

            return NoContent(); //204
        }
    }
}