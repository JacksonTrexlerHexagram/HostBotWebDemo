namespace Hexagram.Saga.Networking {
    public enum SagaResponseCode {
        // OK
        Ok = 200,
        
        // Bad Request
        BadRequest = 400,

        // Unauthorized
        // Invalid token
        // Token has expired
        // User does not exist
        Unauthorized = 401,

        // Not found
        NotFound = 404,

        // Unprocessable entity
        Unprocessable = 422,

        // Server error
        ServerError = 500
    }
}