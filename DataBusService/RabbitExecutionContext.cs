//Copyright 2018 Damir Garipov
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataBusService.Interfaces;
using MassTransit;

namespace DataBusService
{
    public class RabbitExecutionContext: IExecutionContext
    {
        private Dictionary<Type, HandlerInfo> _messageHandlersDictionary;

        public void SetHandlers(Dictionary<Type, HandlerInfo> messageHandlersDictionary)
        {
            _messageHandlersDictionary = messageHandlersDictionary;
        }

        public Task Handler<TMessage>(ConsumeContext<TMessage> context) where TMessage : class
        {
            var messageHandlerType = _messageHandlersDictionary[typeof(TMessage)].MessageHandlerType;
            var constructorInfo = messageHandlerType.GetConstructors()[0];

            BaseMessageHandler<TMessage> messageHandler = null;
            if (DependencyResolver.Current != null)
            {
                var ctorParameters = constructorInfo.GetParameters().Select(p => DependencyResolver.Current.Resolve(p.ParameterType)).ToArray();
                messageHandler = (BaseMessageHandler<TMessage>)constructorInfo.Invoke(ctorParameters);
            }
            else
            {
                messageHandler = (BaseMessageHandler<TMessage>)constructorInfo.Invoke(new object[]{});
            }

            Console.WriteLine($"Task created for handle message {typeof(TMessage)}.");
            return messageHandler.MessageHandle(context.Message);
        }
    }
}