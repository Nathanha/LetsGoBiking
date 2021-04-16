
let mymap = null;
let markers = [];
let lines = [];

$(() => {
    $('#input_start_address').val('37 Rue de la Bastille, 44000 Nantes');
    $('#input_end_address').val('Place des Enfants Nantais, 44000 Nantes');
    $('#error-message').hide();
    $('#loading-message').hide();
    $('.routing-title-container').hide();
    
    mymap = L.map('mapid').setView([47.218932, -1.5692016], 13);

    L.tileLayer('https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
    }).addTo(mymap);

    mymap.invalidateSize(true);
});

function getItinerary() {
    resetData();

    var url = "http://localhost:8733/Design_Time_Addresses/LetsGoBiking/RoutingWithBikes/REST/GetItineraryBetweenTwoAddress?start=" + 
    $('#input_start_address').val() + 
    "&end=" + $('#input_end_address').val() ;

    $('#loading-message').show();

    $.getJSON(url,(routings) => {

        $('#loading-message').hide();

        let routing_time = 0;

        if (routings.GetItineraryBetweenTwoAddressResult.length > 0) {
            
            // Stating Point
            addMarkerToMap(routings.GetItineraryBetweenTwoAddressResult[0].waypoints[0].latitude, routings.GetItineraryBetweenTwoAddressResult[0].waypoints[0].longitude, 'start');

            var items = '';
            routings.GetItineraryBetweenTwoAddressResult.forEach((routing) => {
                let type;
                if (routing.type == "Foot" && routing.waypoints[1].station != null)
                {
                    items += getTitle('mdi:walk', Math.floor(routing.duration/60), 'Routing to '+ routing.waypoints[1].station.name);
                    type = 'start';
                    // addMarkerToMap(routing.waypoints[0].latitude, routing.waypoints[0].longitude, 'start');
                } else if (routing.type == "Foot")
                {
                    items += getTitle('mdi:walk', Math.floor(routing.duration/60), 'Routing to destination');
                    type = 'end';
                    addMarkerToMap(routing.waypoints[1].latitude, routing.waypoints[1].longitude, type);
                } else if (routing.type == "Bike")
                {
                    items += getTitle('mdi:bike', Math.floor(routing.duration/60), 'Routing to '+ routing.waypoints[1].station.name);
                    type = 'bike';
                    addMarkerToMap(routing.waypoints[0].latitude, routing.waypoints[0].longitude, type, routing.waypoints[0].station);
                    addMarkerToMap(routing.waypoints[1].latitude, routing.waypoints[1].longitude, type, routing.waypoints[1].station);
                }
    
                routing_time += Math.floor(routing.duration/60);
    
                var waypoints = [];
                routing.steps.forEach((step) => {
                    let text = '<li class="list-group-item">'+ step.type;
                    if (step.modifier != null) text += " " + step.modifier;
                    if (step.name) text += " on " + step.name;
                    if (step.distance > 0) text += " in " + step.distance + "m";
                    text += '</li>';
                    items += text;
                    waypoints.push(L.latLng(step.location.latitude, step.location.longitude));
                });
                addRouting(waypoints, type);
            });
    
            $('.routing-title-container').show();
            $('#routing-title').append('Routing of ' + routing_time + ' min');
    
            $('.directions-skeleton').hide();
            $('#directions_list').append(items);
        } else {
            $('#error-message').show();
        }

    }); 
}

/**
 * Reset all data on the map and the itinerary list
 */
function resetData() {
    if (markers.length > 0) {
        markers.forEach((marker) => {
            marker.removeFrom(mymap);
        });
    }
    if (lines.length > 0) {
        lines.forEach((line) => {
            line.removeFrom(mymap);
        });
    }
    markers = [];
    lines = [];
    $('#routing-title').empty();
    $('#directions_list').empty();
    $('.directions-skeleton').show();
    $('#error-message').hide();
    $('#loading-message').hide();
    $('.routing-title-container').hide();
}

/**
 * Get a title to display in the itineray list with the given parameters
 * @param {*} icon 
 * @param {*} duration 
 * @param {*} title 
 * @returns 
 */
function getTitle(icon, duration, title) {
    return '<li class="list-group-item title">'+
           '<div>'+
           '<span class="icon-title iconify" data-icon="'+icon+'" data-inline="false"></span>'+
           duration+' min : '+
           '<span class="list-title-text">'+title+'</span>'+
           '</div>'+
           '</li>';
}

/**
 * Add a marker on the map with the given latitude and longitude
 * @param {*} latitude 
 * @param {*} longitude 
 * @param {*} type 
 * @param {*} station 
 */
function addMarkerToMap(latitude, longitude, type, station = null) {
    // create popup contents
    var customPopup;

    // specify popup options 
    var customOptions = {
        maxWidth: 200,
        className : 'popupCustom'
    }

    var className = 'marker';
    var icon;
    if (type == 'start') {
        customPopup = '<p class="popup-title">Starting point</p>';
        className += ' marker-start';
        icon = 'mdi:flag';
    } 
    else if (type == 'end') {
        customPopup = '<p class="popup-title">Ending point</p>';
        className += ' marker-end';
        icon = 'mdi:flag-checkered';
    }
    else if (type == 'bike') {
        customPopup = '<p class="popup-title">'+ station.name +'</p>'+
                      '<p style="margin-top: 5px" class="popup-text">Number of bike available : '+station.totalStands.availabilities.bikes+'</p>'+
                      '<p class="popup-text">Number of electrical bike available : '+station.totalStands.availabilities.electricalBikes+'</p>';
        className += ' marker-done';
        icon = 'mdi:bike';
    }

    var markerIcon = L.divIcon({
        className: className,
        iconSize: [28, 28],
        iconAnchor: [19, 19],
        popupAnchor:  [-6, -14],
        // html: '<div class="icon">1</div>'
        html: '<span class="icon iconify" data-icon="'+icon+'" data-inline="false"></span>'
    });
    markers.push(L.marker([latitude, longitude], {icon: markerIcon}).bindPopup(customPopup,customOptions).addTo(mymap));

}

/**
 * Add a line on the map between the two waypoints
 * @param {*} waypoints 
 * @param {*} type 
 */
function addRouting(waypoints, type) {
    let color;
    if (type == 'start') color = '#d21d1d';
    else if (type == 'end') color = '#35b626';
    else if (type == 'bike') color = '#9ca89b';
    
    lines.push(L.polyline(waypoints, {color: color}).addTo(mymap)); 
}