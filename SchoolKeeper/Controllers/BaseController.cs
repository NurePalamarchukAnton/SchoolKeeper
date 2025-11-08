using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolKeeper.Abstractions.Interfaces.Repository;

namespace SchoolKeeper.Controllers
{

    [Authorize]
    public class BaseController<T, TDto>(IGenericRepository<T> _repo) : ControllerBase 
        where T : BaseModel
        where TDto : class
    {

        /// <summary>Отримати список усіх сутностей (з пагінацією)</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 50;

            var query = await _repo.GetAllAsync();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var dtos = items.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>Отримати сутність за Id</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<TDto>> GetById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(MapToDto(entity));
        }

        /// <summary>Створити сутність</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<ActionResult<TDto>> Create([FromBody] TDto dto)
        {
            if (dto == null) return BadRequest();

            var entity = MapToEntity(dto);
            await OnBeforeCreate(entity);
            var created = await _repo.AddAsync(entity);
            await OnAfterCreate(created);

            return CreatedAtAction(nameof(GetById), new { id = GetEntityId(created) }, MapToDto(created));
        }

        /// <summary>Оновити сутність за Id</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<TDto>> Update(int id, [FromBody] TDto dto)
        {
            if (dto == null) return BadRequest("DTO is null.");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Создаем временную сущность для хука (если нужна)
            var tempEntity = MapToEntity(dto);
            await OnBeforeUpdate(existing, tempEntity);
            
            // Обновляем существующую сущность
            MapToEntity(dto, existing);

            var updated = await _repo.UpdateAsync(existing);
            await OnAfterUpdate(updated);

            return Ok(MapToDto(updated));
        }

        /// <summary>Видалити сутність за Id</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();

            await OnBeforeDelete(entity);
            await _repo.RemoveAsync(entity);
            await OnAfterDelete(id);

            return NoContent();
        }

        // -------- Mapping methods (можно переопределять в наследниках) --------
        /// <summary>Преобразовать Entity в DTO</summary>
        protected virtual TDto MapToDto(T entity)
        {
            // Базовая реализация через рефлексию (можно переопределить в наследниках)
            var dto = Activator.CreateInstance<TDto>();
            if (dto == null) throw new InvalidOperationException($"Failed to create instance of {typeof(TDto).Name}");
            
            var entityType = typeof(T);
            var dtoType = typeof(TDto);

            foreach (var prop in entityType.GetProperties())
            {
                // Пропускаем навигационные свойства (коллекции)
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && 
                    prop.PropertyType != typeof(string))
                    continue;

                var dtoProp = dtoType.GetProperty(prop.Name);
                if (dtoProp != null && dtoProp.CanWrite && prop.CanRead)
                {
                    try
                    {
                        var value = prop.GetValue(entity);
                        
                        // Проверяем совместимость типов
                        if (value != null)
                        {
                            if (dtoProp.PropertyType.IsAssignableFrom(prop.PropertyType) ||
                                (Nullable.GetUnderlyingType(dtoProp.PropertyType) == prop.PropertyType) ||
                                (Nullable.GetUnderlyingType(prop.PropertyType) == dtoProp.PropertyType) ||
                                dtoProp.PropertyType == prop.PropertyType)
                            {
                                dtoProp.SetValue(dto, value);
                            }
                        }
                        else if (Nullable.GetUnderlyingType(dtoProp.PropertyType) != null)
                        {
                            // Для nullable типов можно установить null
                            dtoProp.SetValue(dto, null);
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки маппинга для отдельных свойств
                        continue;
                    }
                }
            }

            return dto;
        }

        /// <summary>Преобразовать DTO в Entity</summary>
        protected virtual T MapToEntity(TDto dto)
        {
            // Базовая реализация через рефлексию (можно переопределить в наследниках)
            var entity = Activator.CreateInstance<T>();
            if (entity == null) throw new InvalidOperationException($"Failed to create instance of {typeof(T).Name}");
            
            MapToEntity(dto, entity);
            return entity;
        }

        /// <summary>Преобразовать DTO в существующую Entity (обновление свойств)</summary>
        protected virtual void MapToEntity(TDto dto, T entity)
        {
            var entityType = typeof(T);
            var dtoType = typeof(TDto);

            foreach (var dtoProp in dtoType.GetProperties())
            {
                // Пропускаем Id при обновлении, чтобы не перезаписать его
                if (dtoProp.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    continue;

                var entityProp = entityType.GetProperty(dtoProp.Name);
                if (entityProp != null && entityProp.CanWrite && dtoProp.CanRead)
                {
                    try
                    {
                        var value = dtoProp.GetValue(dto);
                        if (value != null)
                        {
                            // Проверяем совместимость типов
                            if (entityProp.PropertyType.IsAssignableFrom(dtoProp.PropertyType) ||
                                (Nullable.GetUnderlyingType(entityProp.PropertyType) == dtoProp.PropertyType) ||
                                (Nullable.GetUnderlyingType(dtoProp.PropertyType) == entityProp.PropertyType))
                            {
                                entityProp.SetValue(entity, value);
                            }
                        }
                        else if (Nullable.GetUnderlyingType(entityProp.PropertyType) != null)
                        {
                            // Для nullable типов можно установить null
                            entityProp.SetValue(entity, null);
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки маппинга для отдельных свойств
                        continue;
                    }
                }
            }
        }

        // -------- Hooks (можно переопределять в наследниках) --------
        protected virtual Task OnBeforeCreate(T entity) => Task.CompletedTask;
        protected virtual Task OnAfterCreate(T entity) => Task.CompletedTask;
        protected virtual Task OnBeforeUpdate(T existing, T incoming) => Task.CompletedTask;
        protected virtual Task OnAfterUpdate(T entity) => Task.CompletedTask;
        protected virtual Task OnBeforeDelete(T entity) => Task.CompletedTask;
        protected virtual Task OnAfterDelete(int id) => Task.CompletedTask;

        // -------- Helpers --------
        protected virtual int GetEntityId(T entity)
        {
            var prop = typeof(T).GetProperty("Id");
            if (prop != null && prop.PropertyType == typeof(int))
                return (int)(prop.GetValue(entity) ?? 0);

            return 0;
        }

        protected virtual void SetEntityId(T entity, int id)
        {
            var prop = typeof(T).GetProperty("Id");
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(int))
                prop.SetValue(entity, id);
        }
    }
}
