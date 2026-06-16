using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Bolao.Application.Common.Exceptions;
using FluentValidation;

namespace Bolao.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var traceId = context.TraceIdentifier;

        var firebaseUid =
            context.User.FindFirstValue("firebase_uid")
            ?? context.User.FindFirstValue("uid")
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "anonymous";

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro interno não tratado. TraceId: {TraceId}. Method: {Method}. Path: {Path}. StatusCode: {StatusCode}. FirebaseUid: {FirebaseUid}",
                traceId,
                context.Request.Method,
                context.Request.Path,
                (int)statusCode,
                firebaseUid);
        }
        else
        {
            _logger.LogWarning(
                "Erro tratado. TraceId: {TraceId}. Method: {Method}. Path: {Path}. StatusCode: {StatusCode}. FirebaseUid: {FirebaseUid}. Message: {Message}",
                traceId,
                context.Request.Method,
                context.Request.Path,
                (int)statusCode,
                firebaseUid,
                exception.Message);
        }

        var response = new
        {
            message = GetMessage(exception),
            statusCode = (int)statusCode,
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            ForbiddenException => HttpStatusCode.Forbidden,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            ValidationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private static string GetMessage(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                string.Join(" | ", validationException.Errors.Select(x => x.ErrorMessage)),

            _ => exception.Message
        };
    }
}