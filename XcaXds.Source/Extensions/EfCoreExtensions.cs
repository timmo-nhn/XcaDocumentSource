using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Extensions;

public class EfCoreExtensions
{
    //public override int SaveChanges()
    //{
    //    foreach (var entry in ChangeTracker.Entries<DbDocumentEntry>())
    //    {
    //        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
    //        {
    //            var doc = entry.Entity;
    //            doc.PatientKey = $"{doc.SourcePatientInfo.PatientSystem}|{doc.SourcePatientInfo.PatientId}";
    //        }
    //    }

    //    return base.SaveChanges();
    //}
}
