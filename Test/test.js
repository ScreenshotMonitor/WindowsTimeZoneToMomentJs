var moment = require('moment');
var momentTz = require('moment-timezone');
require('./node_modules/moment-timezone/moment-timezone-utils.js');
var _ = require('underscore');

var log = console.log;

function overAll(_y, _m, _d, _h, _mn, callback){
	var isBreak = false;
	_.each(_y, function(y){
	if (isBreak) return;
	_.each(_m, function(m){
	if (isBreak) return;
	_.each(_d, function(d){
	if (isBreak) return;
	_.each(_h, function(h){
	if (isBreak) return;
	_.each(_mn, function(mn){
	if (isBreak) return;
	isBreak = callback(y,m,d,h,mn);
	});
	});
	});
	});
	});
}

function lp(x){
	return (x<10 ? "0": "")+x;
}

function test_zone(lt){
  log("-----------------------------------");
  var verbose = true;
  var err = false;
  if (verbose) log("Testing : "+lt.name);
  t = moment.tz.zone(lt.IanaId);
  if (!t) {
  	if (verbose) log("Warning: Moment.js time zone ["+lt.Id+"] not found - Skip");
  	return;
  };

  var packed = moment.tz.pack(lt);
  moment.tz.add(packed);
   
  var years = _.range(moment().year()-1, moment().year()+1);
  var months = _.range(1, 12);
  var days = _.range(1, 28, 2);
  overAll(years, months, days, _.range(0, 23, 3), _.range(0, 59, 15), function(y,m,d,h,mn){
	var now = y+"-"+lp(m)+"-"+lp(d)+"T"+lp(h)+":"+lp(mn)+":00";
	var z1 = moment.tz(now, t.name);
	var z2 = moment.tz(now, lt.name);
	var s1 = z1.format(); 
	var s2 = z2.format(); 
	
	if (s1 != s2){
		err = true;
		log("Fail: "+s1 +" <> "+ s2 + " for zone : "+lt.name+" ( utc : "+now+" )");		
	}
	return err;
  });
  
  if (!err){  	
  	log(lt.name + " - OK ("+lt.untils.length+" rules)");
  	return true;
  };
  
};

function do_test(){
  var winTz = require('./wintz.json');
  var e = 0;
  var n = 0;
  _.each(winTz, function(x){
  	n++;
  	if (test_zone(x)) e++;
  });
  log("")
  log("RESULT: "+e+" - OK, "+(n-e)+" - FAIL ( "+(Math.round(100.0*e/n)+"% success )"));
};

do_test();