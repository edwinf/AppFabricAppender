AppFabricAppender
=================

1) Overview
	A log4net appender that pushes log entries to an AppFabric named Cache. 

2) License: 
	This appender is licensed under the apache 2 license.  Which basically means: 
		a) use it / modify it however you want as long as you leave the copyright notice in the source.
		b) If you're just using the DLLs, you don't need to worry about anything.

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

3) Sample Config:

<log4net>
	<appender name="AppFabricAppender" type="log4netAppenders.AppFabricAppender, AppFabricAppender-log4net">
		<host>
			<host>127.0.0.1</host>
			<port>22233</port>
		</host>
		<layout type="log4net.Layout.PatternLayout" value="%date [%thread] %-5level %logger - %message%newline" />
	</appender>
	<root>
		<level value="ALL" />
		<appender-ref ref=""AppFabricAppender"" />
	</root>
</log4net>