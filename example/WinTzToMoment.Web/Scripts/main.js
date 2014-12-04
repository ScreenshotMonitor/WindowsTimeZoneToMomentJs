(function() {
    
    _.each(window.windowsTimezones, function (tz) {
        console.log("Added time zone:", tz.name);
        var packed = moment.tz.pack(tz);
        moment.tz.add(packed);
    });
    
    var p = {};

    
    function convert() {
        $(".res-marker").hide();
        var tzId = $("#winTz").val();
        var sUtc = $("#dateTime").val().trim();
        console.log(tzId);
        
        var s1 = "Moment Timezone has no data for " + tzId;
        if (moment.tz(tzId)) {
            var tUtc = moment.utc(sUtc);
            var tZ = moment.tz(tUtc, tzId);
            s1 = tZ.format();
        }
        
        $("#by-server").html("loading...");
        $("#by-client").html(s1);
        $("#result").show();
        $.ajax({ url: p.convertUrl, cache: false, data: {
            id: tzId, dt: sUtc
        } }).done(function (s2) {
            $("#by-server").html(s2);
            if (s1 === s2) {
                $("#ok").show();
            } else {
                $("#fail").show();
            }
        });
    }
    
    $(function() {
        p.convertUrl = $("#by-server").data("url");
        $("#convert").click(convert);

        $("#winTz").empty();
        _.each(window.windowsTimezones, function (tz) {
            $("#winTz").append("<option value='"+tz.name+"'>"+tz.name+"</option>");
        });
        
    });
})()