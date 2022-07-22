using System;

namespace Hexagram.Saga {

    public class SagaException : Exception {
        public SagaException() : base() { }
        public SagaException(string message) : base(message) { }
    }

    public class UninitializedException : SagaException {
        public UninitializedException() : base() { }
        public UninitializedException(string message) : base(message) { }
    }

    public class BadRequestException : SagaException {
        public BadRequestException() : base() { }
        public BadRequestException(string message) : base(message) { }
    }

    public class NotFoundException : SagaException {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
    }
    
    public class UnprocessableException : SagaException {
        public UnprocessableException() : base() { }
        public UnprocessableException(string message) : base(message) { }
    }
    
    public class UnauthorizedException : SagaException {
        public UnauthorizedException() : base() { }
        public UnauthorizedException(string message) : base(message) { }
    }
    
    public class ServerException : SagaException {
        public ServerException() : base() { }
        public ServerException(string message) : base(message) { }
    }
}