using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Data;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Implements
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly AppDbContext _context;

        public JobApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication> AddAsync(JobApplication application)
        {
            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<JobApplication?> GetByIdAsync(int applicationId)
        {
            return await _context
                .JobApplications.Include(ja => ja.Student)
                .ThenInclude(s => s.Student)
                .Include(ja => ja.JobOffer)
                .FirstOrDefaultAsync(ja => ja.Id == applicationId);
        }

        public async Task<JobApplication?> GetByStudentAndOfferAsync(int studentId, int offerId)
        {
            return await _context
                .JobApplications.Include(ja => ja.Student)
                .Include(ja => ja.JobOffer)
                .FirstOrDefaultAsync(ja => ja.StudentId == studentId && ja.JobOfferId == offerId);
        }

        public async Task<IEnumerable<JobApplication>> GetByStudentIdAsync(int studentId)
        {
            return await _context
                .JobApplications.Include(ja => ja.JobOffer)
                .Include(ja => ja.Student)
                .ThenInclude(s => s.Student)
                .Where(ja => ja.StudentId == studentId)
                .OrderByDescending(ja => ja.ApplicationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobApplication>> GetByOfferIdAsync(int offerId)
        {
            return await _context
                .JobApplications.Include(ja => ja.Student)
                .ThenInclude(s => s.Student)
                .Include(ja => ja.JobOffer)
                .Where(ja => ja.JobOfferId == offerId)
                .OrderByDescending(ja => ja.ApplicationDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(JobApplication application)
        {
            _context.JobApplications.Update(application);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

    }
}
