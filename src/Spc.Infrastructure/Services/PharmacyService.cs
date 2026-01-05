using Microsoft.EntityFrameworkCore;
using Spc.Application.Pharmacies;
using Spc.Domain.Entities;
using Spc.Infrastructure.Data;
using Spc.Application.Common.Exceptions;

namespace Spc.Infrastructure.Services;

public sealed class PharmacyService : IPharmacyService
{
    private readonly SpcDbContext _db;
    public PharmacyService(SpcDbContext db) => _db = db;

    public async Task<PharmacyDto> CreateAsync(CreatePharmacyRequest request, CancellationToken ct)
    {
        if (await _db.Pharmacies.AnyAsync(p => p.RegNo == request.RegNo, ct)) 
            throw new ConflictException("Pharmacy RegNo already exists.");

        var pharmacy = new Pharmacy 
        { 
            Name = request.Name.Trim(), 
            Type = request.Type, 
            RegNo = request.RegNo.Trim(), 
            Address = request.Address.Trim() 
        };

        _db.Pharmacies.Add(pharmacy);
        await _db.SaveChangesAsync(ct);

        return new PharmacyDto 
        { 
            PharmacyId = pharmacy.PharmacyId, 
            Name = pharmacy.Name, 
            Type = pharmacy.Type, 
            RegNo = pharmacy.RegNo, 
            Address = pharmacy.Address 
        };
    }

    public async Task<PharmacyDto?> GetByIdAsync(Guid pharmacyId, CancellationToken ct)
    {
        return await _db.Pharmacies.AsNoTracking().Where(p => p.PharmacyId == pharmacyId)
            .Select(p => new PharmacyDto 
            { 
                PharmacyId = p.PharmacyId, 
                Name = p.Name, 
                Type = p.Type, 
                RegNo = p.RegNo, 
                Address = p.Address 
            })
            .FirstOrDefaultAsync(ct);
    }
}