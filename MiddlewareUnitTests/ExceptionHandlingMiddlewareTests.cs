using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WebApi.Middleware;
using WebApi.Middleware.Exceptions;

namespace MiddlewareUnitTests
{
    [TestFixture]
    public class ExceptionHandlingMiddlewareTests
    {
        private ILoggerFactory _loggerFactory;
        private Mock<IResponseWriter> _mockResponseWriter;
        private Mock<HttpContext> _mockContext;
        private Mock<HttpResponse> _mockResponse;
        private Mock<HttpRequest> _mockRequest;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        }

        [SetUp]
        public void Setup()
        {
            _mockResponseWriter = new Mock<IResponseWriter>();
            _mockContext = new Mock<HttpContext>();
            _mockResponse = new Mock<HttpResponse>();
            _mockRequest = new Mock<HttpRequest>();
            _mockContext.SetupGet(m => m.Request).Returns(_mockRequest.Object);
            _mockContext.SetupGet(m => m.Response).Returns(_mockResponse.Object);
        }

        [Test]
        public async Task InvokeAsync_DelegateThrowsHandledException_ExactMapperExists_WritesProblemDetailsToResponseStream()
        {
            // Arrange
            var notSupportedException = new NotSupportedException("I should not be present in the response.");
            RequestDelegate request = async context =>
            {
                await Task.Delay(1);
                throw notSupportedException;
            };

            var mockMapper = new Mock<IProblemDetailsMapper>();
            mockMapper.SetupGet(m => m.ExceptionType).Returns(typeof(NotSupportedException));
            mockMapper.Setup(m => m.CanHandle(notSupportedException)).Returns(true);

            const int statusCode = 500;
            var problemDetails = new ProblemDetails
            {
                Instance = "https://oursite.net/issue/foo",
                Status = statusCode,
                Type = "https://oursite.net/problems/notsupported",
                Detail = "The current operation is not currently supported.",
                Title = "Operation not supported"
            };
            mockMapper.Setup(m => m.MapToProblemDetails(notSupportedException)).Returns(problemDetails);

            var middleware = new ExceptionHandlingMiddleware(
                _loggerFactory.CreateLogger<ExceptionHandlingMiddleware>(),
                new[] {mockMapper.Object},
                _mockResponseWriter.Object);

            // Act
            await middleware.InvokeAsync(_mockContext.Object, request);

            // Assert
            _mockResponseWriter.Verify(m => m.WriteToResponseAsync(_mockResponse.Object, problemDetails, statusCode, default), Times.Once);
        }
        
        [Test]
        public async Task InvokeAsync_DelegateThrowsHandledException_MapperForBaseTypeExists_WritesProblemDetailsToResponseStream()
        {
            // Arrange
            var notSupportedException = new NotSupportedException("I should not be present in the response.");
            RequestDelegate request = async context =>
            {
                await Task.Delay(1);
                throw notSupportedException;
            };

            var mockMapper = new Mock<IProblemDetailsMapper>();
            mockMapper.SetupGet(m => m.ExceptionType).Returns(typeof(NotSupportedException).BaseType);
            mockMapper.Setup(m => m.CanHandle(notSupportedException)).Returns(true);

            const int statusCode = 503;
            var problemDetails = new ProblemDetails
            {
                Instance = "https://oursite.net/issue/foo",
                Status = statusCode,
                Type = "https://oursite.net/problems/systemproblem",
                Detail = "The system is having a bad day, sorry.",
                Title = "Whoops!"
            };
            mockMapper.Setup(m => m.MapToProblemDetails(notSupportedException)).Returns(problemDetails);

            var middleware = new ExceptionHandlingMiddleware(
                _loggerFactory.CreateLogger<ExceptionHandlingMiddleware>(),
                new[] {mockMapper.Object},
                _mockResponseWriter.Object);

            // Act
            await middleware.InvokeAsync(_mockContext.Object, request);

            // Assert
            _mockResponseWriter.Verify(m => m.WriteToResponseAsync(_mockResponse.Object, problemDetails, statusCode, default), Times.Once);
        }
    }
}