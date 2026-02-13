using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Source.Services;
using XcaXds.Source.Source;

namespace XcaXds.WebService.Controllers;

[Tags("RESTful Registry/Repository (CRUD)")]
[ApiController]
[Route("api/rest")]
public class RestfulRegistryRepositoryController : ControllerBase
{
    private readonly ILogger<RestfulRegistryRepositoryController> _logger;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly IVariantFeatureManager _featureManager;

    public RestfulRegistryRepositoryController(
        ILogger<RestfulRegistryRepositoryController> logger,
        RestfulRegistryRepositoryService registryRestfulService,
        IVariantFeatureManager featureManager,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper
        )
    {
        _logger = logger;
        _restfulRegistryService = registryRestfulService;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _featureManager = featureManager;
    }

    [Produces("application/json")]
    [HttpGet("document-list")]
    public async Task<IActionResult> GetDocumentList(string? id, string? status, DateTime serviceStartTimeFrom, DateTime serviceStopTimeTo, int pageNumber = 1, int pageSize = 10)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Read")) return NotFound();

        if (string.IsNullOrWhiteSpace(id) && Request.Headers.TryGetValue("x-patient-id", out var patientId))
        {
            id = patientId;
        }

        var requestTimer = Stopwatch.StartNew();

        var entries = _restfulRegistryService.GetDocumentListForPatient(id, status, serviceStartTimeFrom, serviceStopTimeTo, pageNumber, pageSize);

        requestTimer.Stop();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: document-list");

        if (entries.Success)
        {
            _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully retrieved {entries.DocumentListEntries.Count} entries in {requestTimer.ElapsedMilliseconds} ms");
            return Ok(entries);
        }

        for (int i = 0; i < entries.Errors.Count; i++) _logger.LogError($"{Request.HttpContext.TraceIdentifier}\n######## Error #{i} ########\n ErrorCode: {entries.Errors[i].Code}\n Message: {entries.Errors[i].Message}");

        return BadRequest(entries);
    }

    [Produces("application/json")]
    [HttpGet("document-history")]
    public async Task<IActionResult> GetDocumentHistory(string? id, bool minimal)
    {
        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Received request for action: document-entry");

        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Read")) return NotFound();

        if (string.IsNullOrWhiteSpace(id) && Request.Headers.TryGetValue("x-patient-id", out var patientId))
        {
            id = patientId;
        }

        var requestTimer = Stopwatch.StartNew();
        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var allAssociations = documentRegistry.OfType<AssociationDto>().ToList();
        var allDocuments = documentRegistry.OfType<DocumentEntryDto>().ToList();
        var allSubmissionSets = documentRegistry.OfType<SubmissionSetDto>().ToList();

        var visitedDocs = new HashSet<string>();
        var visitedAssociations = new HashSet<string>();

        var queue = new Queue<string>();
        queue.Enqueue(id);
        visitedDocs.Add(id);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            var relatedAssociations = allAssociations
                .Where(a =>
                    (a.SourceObject == currentId || a.TargetObject == currentId)
                    && !string.IsNullOrWhiteSpace(a.AssociationType) && a.AssociationType.IsAnyOf(Constants.Xds.AssociationType.Replace, Constants.Xds.AssociationType.Addendum))
                .ToList();

            foreach (var assoc in relatedAssociations)
            {
                if (visitedAssociations.Add(assoc.Id))
                {
                    var otherId = assoc.SourceObject == currentId
                        ? assoc.TargetObject
                        : assoc.SourceObject;

                    if (visitedDocs.Add(otherId))
                    {
                        queue.Enqueue(otherId);
                    }
                }
            }
        }

        var relatedDocuments = allDocuments
        .Where(d => visitedDocs.Contains(d.Id))
        .ToList();

        var relatedAssociationsa = allAssociations
            .Where(a => visitedAssociations.Contains(a.Id))
            .ToList();

        var relatedSubmissionSets = allSubmissionSets
            .Where(ss =>
                allAssociations.Any(a =>
                    visitedDocs.Contains(a.SourceObject) &&
                    a.SourceObject == ss.Id))
            .ToList();

        var result = new List<RegistryObjectDto>();

        result.AddRange(
            allDocuments.Where(d => visitedDocs.Contains(d.Id)));

        result.AddRange(
            allAssociations.Where(a => visitedAssociations.Contains(a.Id)));

        result.AddRange(
            allSubmissionSets.Where(ss =>
                allAssociations.Any(a =>
                    visitedDocs.Contains(a.SourceObject) &&
                    a.SourceObject == ss.Id)));

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: document-entry");
        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully retrieved {result.Count} items in {requestTimer.ElapsedMilliseconds} ms");
        requestTimer.Stop();

        if (minimal)
        {
            var minimalResult = result.Select(obj =>
            {
                return obj switch
                {
                    DocumentEntryDto doc =>
                        new MinimalRegistryObject(doc.Id, doc.AvailabilityStatus, "DocumentEntryDto"),

                    SubmissionSetDto ss =>
                        new MinimalRegistryObject(ss.Id, ss.AvailabilityStatus, "SubmissionSetDto"),

                    AssociationDto assoc =>
                        new MinimalRegistryObject(assoc.Id, assoc.AssociationType, "AssociationDto"),

                    _ =>
                        new MinimalRegistryObject(obj.Id, null, "Unknown")
                };
            });

            return Ok(minimalResult);
        }

        return Content(RegistryJsonSerializer.Serialize(result), Constants.MimeTypes.Json);
    }

    [Produces("application/json")]
    [HttpGet("document-entry")]
    public async Task<IActionResult> GetDocumentEntry(string? id)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Read")) return NotFound();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Received request for action: document-entry");

        if (string.IsNullOrWhiteSpace(id) && Request.Headers.TryGetValue("x-patient-id", out var patientId))
        {
            id = patientId;
        }

        var requestTimer = Stopwatch.StartNew();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var association = documentRegistry.OfType<AssociationDto>().FirstOrDefault(assoc => assoc.SourceObject == id || assoc.TargetObject == id);

        if (association == null) return NotFound();


        var documentEntry = documentRegistry.OfType<DocumentEntryDto>().FirstOrDefault(docEnt => docEnt.Id == association?.TargetObject);
        var submissionSet = documentRegistry.OfType<SubmissionSetDto>().FirstOrDefault(docEnt => docEnt.Id == association?.SourceObject);

        var documentReference = new DocumentReferenceDto()
        {
            Association = association,
            DocumentEntry = documentEntry,
            SubmissionSet = submissionSet,
            Document = new()
            {
                Data = _repositoryWrapper.GetDocumentFromRepository(documentEntry.HomeCommunityId, documentEntry.RepositoryUniqueId, documentEntry.UniqueId),
                DocumentId = documentEntry.UniqueId
            }
        };
        requestTimer.Stop();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: document-entry");

        if (documentEntry != null)
        {
            _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully retrieved {documentEntry.Id} in {requestTimer.ElapsedMilliseconds} ms");
            return Content(RegistryJsonSerializer.Serialize(documentReference), Constants.MimeTypes.Json);
        }

        return NotFound();
    }

    [Produces("application/json")]
    [HttpGet("document")]
    public async Task<IActionResult> GetDocument([FromQuery] string? home, [FromQuery] string? repository, [FromQuery] string? document)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Read")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var entries = _restfulRegistryService.GetDocument(home, repository, document);

        requestTimer.Stop();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: get-document");

        if (entries.Document != null)
        {
            _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully retrieved document with id: {entries.Document.DocumentId} in {requestTimer.ElapsedMilliseconds} ms");
            return Ok(entries);
        }
        else
        {
            _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully retrieved 0 documents in {requestTimer.ElapsedMilliseconds} ms");
            return Ok(entries);
        }
    }

    [Produces("application/json")]
    [HttpGet("document-status")]
    public async Task<IActionResult> GetDocumentStatus([FromQuery] string? home, [FromQuery] string? repository, [FromQuery] string? document)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Read")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var documentStatus = _restfulRegistryService.GetDocumentStatus(home, repository, document);

        //if (string.IsNullOrWhiteSpace(id) && Request.Headers.TryGetValue("x-patient-id", out var patientId))
        //{
        //	id = patientId;
        //}		


        requestTimer.Stop();

        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Completed action: get-document-status");

        return Ok(documentStatus);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPost("document-entry")]
    public async Task<IActionResult> UploadDocument([FromBody] DocumentReferenceDto documentReferenceDto)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Create")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var uploadResponse = _restfulRegistryService.UploadDocumentAndMetadata(documentReferenceDto);

        requestTimer.Stop();

        if (uploadResponse.Success)
        {
            _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully uploaded document and metadata in {requestTimer.ElapsedMilliseconds} ms");
            return Ok(uploadResponse);
        }

        return BadRequest(uploadResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPut("document-entry")]
    public async Task<IActionResult> UpdateDocument(bool? replace, [FromBody] DocumentReferenceDto documentReference)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Update")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var updateResponse = _restfulRegistryService.UpdateDocumentMetadata(replace, documentReference);

        requestTimer.Stop();

        if (updateResponse.Success)
        {
            _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully updated document and metadata in {requestTimer.ElapsedMilliseconds} ms");
            return Ok(updateResponse);
        }

        return BadRequest(updateResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPatch("document-entry")]
    public async Task<IActionResult> PatchDocument([FromBody] DocumentReferenceDto documentReference)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Update")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var response = _restfulRegistryService.PartiallyUpdateDocumentMetadata(documentReference);

        requestTimer.Stop();

        return Ok(response);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpDelete("document-entry-document")]
    public async Task<IActionResult> DeleteDocument(string id)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Delete")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var deleteResponse = _restfulRegistryService.DeleteDocumentAndMetadata(id);

        requestTimer.Stop();
        _logger.LogInformation($"{Request.HttpContext.TraceIdentifier} - Successfully deleted document: {id}, and metadata in {requestTimer.ElapsedMilliseconds} ms");
        return Ok(deleteResponse);
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpDelete("all-data-for-patient")]
    public async Task<IActionResult> DeleteAllDataForPatient(string patientIdentifier)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Delete")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var deleteResponse = _restfulRegistryService.DeleteAllDataForPatient(patientIdentifier);

        requestTimer.Stop();

        return Ok(deleteResponse);
    }

    [Produces("application/json")]
    [HttpDelete("from-timespan")]
    public async Task<IActionResult> DeleteFromTimeSpan([FromQuery] DateTime timeSpan)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Delete")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var deleteResponse = _restfulRegistryService.DeleteUntilTimeSpan(timeSpan);

        requestTimer.Stop();

        return Ok(deleteResponse);
    }

    [Produces("application/json")]
    [HttpDelete("older-than")]
    public async Task<IActionResult> DeleteOlderThanNDays([FromQuery] TimeUnit unit, [FromQuery] int? days)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Delete")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var deleteResponse = _restfulRegistryService.DeleteOlderThan(unit, days);

        requestTimer.Stop();

        return Ok(deleteResponse);
    }
}