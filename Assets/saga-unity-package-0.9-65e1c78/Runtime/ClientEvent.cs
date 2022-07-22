using System;

namespace Hexagram.Saga {
    public partial class Client {
        
        public delegate void InitializedHandler();
        public delegate void SagaPropertyHandler(SagaProperty property);
        public delegate void MessageHandler(SagaMessage message);

        public event InitializedHandler OnInitialized = delegate { };
    }
}