var moment = require('moment');
var momentTz = require('moment-timezone');
require('./node_modules/moment-timezone/moment-timezone-utils.js');
var _ = require('underscore');

var log = console.log;

function test_zone(item){
  log("-----------------------------------");
  var verbose = true;
  var err = false;
  var lt = item.tz;
  if (verbose) log("Testing : "+lt.name + " ( "+lt.descr+" )");
  
  var packed = moment.tz.pack(lt);
  moment.tz.add(packed);
  
  for(var i = 0; i < item.dates.length; i++){
    if (err) break;
	var x = item.dates[i];
	var tUtc = moment.utc(x.u);
	
	var t1 = moment.tz(tUtc, lt.name);
	var s1 = t1.format(); 
	var s2 = x.z;
    var ss1 = s1.substring(1, 19);
	var ss2 = s2.substring(1, 19);	
	if (ss1 !== ss2){
		err = true;
		log("Fail: Expected ["+s2 +"] but got ["+ s1 + "] for zone : "+lt.name+" ( utc : "+x.u+" )");		
	}
  }
    
  if (!err){  	
  	log(lt.name + " - OK ("+lt.untils.length+" rules)");
  	return true;
  };
  
};

function do_test(){
  var data = require('./test_data.json');
  var e = 0;
  var n = 0;
  _.each(data, function(x){
  	n++;
  	if (test_zone(x)) e++;
  });
  log("")
  log("RESULT: "+e+" - OK, "+(n-e)+" - FAIL ( "+(Math.round(100.0*e/n)+"% success )"));
};

do_test();