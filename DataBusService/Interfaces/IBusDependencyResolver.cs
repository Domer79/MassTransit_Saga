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

namespace DataBusService.Interfaces
{
    public interface IBusDependencyResolver
    {
        /// <summary>
        /// Resolve by generic arguments
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Resolve<TService>();

        /// <summary>
        /// Resolve by serviceType
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolve handler
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageHandlerType"></param>
        /// <returns></returns>
        BaseMessageHandler<TMessage> ResolveHandler<TMessage>(Type messageHandlerType) where TMessage : class;
    }
}