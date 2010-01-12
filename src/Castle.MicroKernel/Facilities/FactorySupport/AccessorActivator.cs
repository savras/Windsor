// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.FactorySupport
{
	using System;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Facilities;


	public class AccessorActivator : DefaultComponentActivator
	{
		public AccessorActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		protected override object Instantiate(CreationContext context)
		{
			String accessor = (String)Model.ExtendedProperties["instance.accessor"];

			PropertyInfo pi = Model.Implementation.GetProperty(
				accessor, BindingFlags.Public | BindingFlags.Static);

			if (pi == null)
			{
				String message = String.Format("You have specified an instance accessor " +
					"for the component '{0}' {1} which could not be found (no public " +
					"static property has this name)", Model.Name, Model.Implementation.FullName);
				throw new FacilityException(message);
			}

			if (!pi.CanRead)
			{
				String message = String.Format("You have specified an instance accessor " +
					"for the component '{0}' {1} which is write-only",
					Model.Name, Model.Implementation.FullName);
				throw new FacilityException(message);
			}

			object instance;
			try
			{
				instance = pi.GetValue(null, new object[0]);
			}
			catch (Exception ex)
			{
				String message = String.Format("The instance accessor " +
					"invocation failed for '{0}' {1}",
					Model.Name, Model.Implementation.FullName);
				throw new FacilityException(message, ex);
			}
			var type = context.Handler.ComponentModel.Implementation;
			if (instance != null && type != null && type.IsInstanceOfType(instance) == false)
			{
				String message =
					String.Format(
						"Factory accessor '{0}.{1}' created instance of type {2}.{5}" +
						"This type is not compatible with implementation type {3} registered for this component ({4}).{5}" +
						"This may signify a bug. If it's the expected behavior, change the registration of this component to cover this return type.",
						pi.DeclaringType.FullName, pi.Name, instance.GetType().FullName, type.FullName, context.Handler.ComponentModel.Name, Environment.NewLine);
				throw new FacilityException(message);
			}
			return instance;
		}
	}
}