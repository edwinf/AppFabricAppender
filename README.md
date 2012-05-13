AppFabricAppender
=================

1. Overview
	A log4net appender that pushes log entries to an AppFabric named Cache. 

2. Sample Config:
<pre><code>&lt;log4net&gt;
	&lt;appender name="AppFabricAppender" type="log4netAppenders.AppFabricAppender, AppFabricAppender-log4net"&gt;
		&lt;host&gt;
			&lt;host&gt;127.0.0.1&lt;/host&gt;
			&lt;port&gt;22233&lt;/port&gt;
		&lt;/host&gt;
		&lt;layout type="log4net.Layout.PatternLayout" value="%date [%thread] %-5level %logger - %message%newline" /&gt;
	&lt;/appender&gt;
	&lt;root&gt;
		&lt;level value="ALL" /&gt;
		&lt;appender-ref ref=""AppFabricAppender"" /&gt;
	&lt;/root&gt;
&lt;/log4net&gt;
</code></pre>

3. To view the entries, there is a simple log reader that is in this source. The code in there can be translated to a console
app pretty simply if that would more suite your needs.  We are in the process of writing a more full featured
cache object viewer in another project: https://github.com/edwinf/WindowsAppFabricObjectViewer.  


4. License: 
   This appender is licensed under the apache 2 license.  Which basically means: 
		a) use it / modify it however you want as long as you leave the copyright notice in the source.
		b) If you're just using the DLLs, you don't need to worry about anything.

   Copyright © 2012 edwinf (https://github.com/edwinf)
   
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
       http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.