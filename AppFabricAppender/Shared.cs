using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace log4netAppenders
{
	internal class Shared
	{
		internal const string LAST_PUSHED_KEY_KEY = "TailLogIndex";

		/// <summary>
		/// used to remove any machine name characters that cause a problem with the cache region name. 
		/// (no idea why these characters cause problems, but they seem to.)
		/// </summary>
		/// <returns></returns>
		internal static string GetMachineName()
		{
			return Environment.MachineName.Replace("-", "");
		}
	}

	
}


/*
 *  Copyright © 2012 edwinf (https://github.com/edwinf)
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
*/

