﻿using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
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

    public RestfulRegistryRepositoryController(ILogger<RestfulRegistryRepositoryController> logger, RestfulRegistryRepositoryService registryRestfulService, IVariantFeatureManager featureManager, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper)
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

        for (int i = 0; i < entries.Errors.Count; i++) _logger.LogError($"{Request.HttpContext.TraceIdentifier} - \n######## Error #{i} ########\n ErrorCode: {entries.Errors[i].Code}\n Message: {entries.Errors[i].Message}");

        return BadRequest(entries);
    }

    [Produces("application/json")]
    [HttpGet("document-entry")]
    public async Task<IActionResult> GetDocumentEntry(string? id)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Read")) return NotFound();

        if (string.IsNullOrWhiteSpace(id) && Request.Headers.TryGetValue("x-patient-id", out var patientId))
        {
            id = patientId;
        }

        var requestTimer = Stopwatch.StartNew();

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var association = documentRegistry.OfType<AssociationDto>().FirstOrDefault(assoc => assoc.SourceObject == id || assoc.TargetObject == id);
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
            return Ok(documentReference);
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
    [Consumes("application/json")]
    [HttpPost("upload")]
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
    [HttpPut("update")]
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
    [HttpPatch("patch")]
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
    [HttpDelete("delete")]
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
    [HttpDelete("delete-all-data-for-patient")]
    public async Task<IActionResult> DeleteAllDataForPatient(string patientIdentifier)
    {
        if (!await _featureManager.IsEnabledAsync("RestfulRegistryRepository_Delete")) return NotFound();

        var requestTimer = Stopwatch.StartNew();

        var deleteResponse = _restfulRegistryService.DeleteAllDataForPatient(patientIdentifier);

        requestTimer.Stop();

        return Ok(deleteResponse);
    }
}
