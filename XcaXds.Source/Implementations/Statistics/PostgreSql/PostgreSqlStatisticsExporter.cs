using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.Source.Models.DatabaseDtos;

namespace XcaXds.Source.Implementations.Statistics.PostGreSql;

public class PostGreSqlStatisticsExporter : IStatisticsExporter
{
    private readonly IDbContextFactory<StatisticsDbContext> _contextFactory;

    public PostGreSqlStatisticsExporter(IDbContextFactory<StatisticsDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task ExportAsync(UserAccessEntry userAccessEntry, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = new DbUserAccessEntry
        {
            SubjectIdHash = userAccessEntry.SubjectIdHash,
            ResourceIdHash = userAccessEntry.ResourceIdHash,
            Success = userAccessEntry.Success,
            SubjectOrganizationCode = userAccessEntry.SubjectOrganization?.Code,
            SubjectOrganizationCodeSystem = userAccessEntry.SubjectOrganization?.CodeSystem,
            SubjectOrganizationDisplayName = userAccessEntry.SubjectOrganization?.DisplayName,
            SubjectOrganizationName = userAccessEntry.SubjectOrganizationName,
            SubjectChildOrganizationCode = userAccessEntry.SubjectChildOrganization?.Code,
            SubjectChildOrganizationCodeSystem = userAccessEntry.SubjectChildOrganization?.CodeSystem,
            SubjectChildOrganizationDisplayName = userAccessEntry.SubjectChildOrganization?.DisplayName,
            SubjectChildOrganizationName = userAccessEntry.SubjectChildOrganizationName,
            AccessTime = userAccessEntry.AccessTime,
            Endpoint = userAccessEntry.Endpoint,
            Action = userAccessEntry.Action,
            AccessBasis = userAccessEntry.AccessBasis,
            ElapsedTimeMillis = userAccessEntry.ElapsedTimeMillis,
            ResponseStatusCode = userAccessEntry.ResponseStatusCode,
            SessionId = userAccessEntry.SessionId,
            Issuer = userAccessEntry.Issuer,
            DocumentConfidentialityCodesJson = userAccessEntry.DocumentConfidentialityCodes is not null
                ? JsonSerializer.Serialize(userAccessEntry.DocumentConfidentialityCodes)
                : null,
            SourceHostName = userAccessEntry.SourceHostName,
            SourceHomeCommunityId = userAccessEntry.SourceHomeCommunityId,
            SourceRepositoryUniqueId = userAccessEntry.SourceRepositoryUniqueId,
            IssuesJson = userAccessEntry.Issues is not null
                ? JsonSerializer.Serialize(userAccessEntry.Issues)
                : null,
            UploadedEntries = userAccessEntry.UploadedEntries,
        };

        context.UserAccessEntries.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
