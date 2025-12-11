using Application.Interfaces;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepo;
        private readonly IUserProgressRepository _userProgressRepo;

        public LessonService(
            ILessonRepository lessonRepo,
            IUserProgressRepository userProgressRepo)
        {
            _lessonRepo = lessonRepo;
            _userProgressRepo = userProgressRepo;
        }

        // Prevent PostgreSQL timestamp errors
        private static DateTime Now() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // -----------------------
        // Mapping Helper
        // -----------------------
        private static LessonDto Map(Lesson l) => new LessonDto
        {
            LessonId = l.LessonId,
            ModuleId = l.ModuleId,
            Title = l.Title?.Trim(),
            Content = l.Content ?? "",
            LessonType = l.LessonType ?? "Text",
            OrderIndex = l.OrderIndex ?? 0,
            Overview = l.Overview ?? "",
            Prerequisites = l.Prerequisites ?? "",
            DurationMinutes = l.DurationMinutes ?? 0
        };

        // -----------------------
        // Get Single
        // -----------------------
        public async Task<LessonDto> GetLessonByIdAsync(Guid lessonId)
        {
            var entity = await _lessonRepo.GetByIdAsync(lessonId);
            return entity == null ? null : Map(entity);
        }

        // -----------------------
        // Get multiple by module (optimized)
        // -----------------------
        public async Task<IEnumerable<LessonDto>> GetLessonsByModuleIdAsync(Guid moduleId)
        {
            var entities = await _lessonRepo.GetByModuleIdAsync(moduleId);

            // Always return sorted & normalized
            return entities
                .OrderBy(x => x.OrderIndex)
                .Select(Map)
                .ToList();
        }

        // -----------------------
        // Create Single Lesson
        // -----------------------
        public async Task<LessonDto> CreateLessonAsync(LessonDto dto)
        {
            var id = dto.LessonId != Guid.Empty ? dto.LessonId : Guid.NewGuid();

            var entity = new Lesson
            {
                LessonId = id,
                ModuleId = dto.ModuleId,
                Title = dto.Title?.Trim(),
                Content = dto.Content,
                LessonType = dto.LessonType ?? "Text",
                OrderIndex = dto.OrderIndex == 0 ? 1 : dto.OrderIndex,
                Overview = dto.Overview,
                Prerequisites = dto.Prerequisites,
                DurationMinutes = dto.DurationMinutes,
                CreatedOn = Now(),
                UpdatedOn = null
            };

            await _lessonRepo.AddAsync(entity);
            return Map(entity);
        }

        // -----------------------
        // Batch create optimized
        // -----------------------
        public async Task<IEnumerable<LessonDto>> CreateLessonsBatchAsync(IEnumerable<LessonDto> lessons)
        {
            // Guarantee stable ordering + consistent IDs
            var list = lessons
                .Select((l, index) => new Lesson
                {
                    LessonId = l.LessonId == Guid.Empty ? Guid.NewGuid() : l.LessonId,
                    ModuleId = l.ModuleId,
                    Title = l.Title?.Trim(),
                    Content = l.Content,
                    LessonType = l.LessonType ?? "Text",
                    OrderIndex = l.OrderIndex == 0 ? index + 1 : l.OrderIndex,
                    Overview = l.Overview,
                    Prerequisites = l.Prerequisites,
                    DurationMinutes = l.DurationMinutes,
                    CreatedOn = Now(),
                    UpdatedOn = null
                })
                .OrderBy(x => x.OrderIndex)
                .ToList();

            await _lessonRepo.AddBatchAsync(list);

            return list.Select(Map).ToList();
        }

        // -----------------------
        // Update Single
        // -----------------------
        public async Task UpdateLessonAsync(LessonDto dto)
        {
            if (dto.LessonId == Guid.Empty)
                throw new Exception("LessonId is required for update.");

            var entity = await _lessonRepo.GetByIdAsync(dto.LessonId);
            if (entity == null)
                throw new Exception("Lesson not found.");

            entity.Title = dto.Title?.Trim();
            entity.Content = dto.Content;
            entity.LessonType = dto.LessonType ?? "Text";
            entity.OrderIndex = dto.OrderIndex == 0 ? entity.OrderIndex : dto.OrderIndex;
            entity.Overview = dto.Overview;
            entity.Prerequisites = dto.Prerequisites;
            entity.DurationMinutes = dto.DurationMinutes;
            entity.UpdatedOn = Now();

            await _lessonRepo.UpdateAsync(entity);
        }

        // -----------------------
        // Delete Lesson
        // -----------------------
        public async Task DeleteLessonAsync(Guid lessonId)
        {
            var entity = await _lessonRepo.GetByIdAsync(lessonId);
            if (entity != null)
                await _lessonRepo.DeleteAsync(entity);
        }

        // -----------------------
        // Mark Lesson Completed
        // -----------------------
        public async Task MarkLessonCompletedAsync(Guid lessonId, Guid userId)
        {
            var lesson = await _lessonRepo.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            await _userProgressRepo.InitializeModuleProgressAsync(
                userId,
                lesson.ModuleId,
                lesson.ModuleId
            );

            var progress = await _userProgressRepo.GetModuleProgressAsync(
                userId,
                lesson.ModuleId,
                lesson.ModuleId
            );

            if (progress != null)
            {
                progress.CurrentLessonId = lesson.LessonId;
                if (progress.Status != "Completed")
                    progress.Status = "InProgress";

                await _userProgressRepo.UpdateModuleProgressAsync(progress);
            }
        }
    }
}
