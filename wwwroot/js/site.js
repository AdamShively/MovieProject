// if not on Import page, set listener for general search
(document.title.substring(0, 6) != "Movie ") ? document.getElementById("searchBtn").addEventListener("click", getMovieId)
    : document.getElementById("importBtn").addEventListener("click", getMovieId)


// Bloodhound with Remote
var movies_suggestions = new Bloodhound({
    datumTokenizer: Bloodhound.tokenizers.whitespace,
    queryTokenizer: Bloodhound.tokenizers.whitespace,
    remote: {
        url: "https://api.themoviedb.org/3/search/movie?api_key=" + apiKey + "&query=%QUERY",
        wildcard: "%QUERY",
        transform: function (response) {
            //console.log(apiKey);
            return response.results;
        }
    },
    //identify: function(item){
    //    return item.id;
    //}
});

// init Typeahead
$('#movieSearch').typeahead(
    {
        // CONFIGURATION OPTIONS
        highlight: true,
        minLength: 2
    },
    {
        // DATASET OPTIONS
        name: "titles",
        source: movies_suggestions, // suggestion engine (bloodhound) is passed as the source
        display: function (item) {
            return item.title;
        },
        limit: 5,
        templates: {
            notFound: '<div>Not Found</div>',
            pending: "<div>Loading...</div>",
            suggestion: function (item) {
                let title = item.title;
                return '<div>' + item.title + '<span id="' + title.toLowerCase() + '" hidden>' + item.id + '</div>';
            }
        }
    });

/* STYLING Twitter Typeahead */
$(".twitter-typeahead").addClass("form-control").css("padding", 0);
$(".tt-hint,.tt-input").addClass("w-100");
$(".tt-hint").css("opacity", .6);

function getMovieId() {
    let formId = "#searchForm", action = "Details";
    if (document.title.substring(0, 6) == "Movie ") {
        formId = "#importForm";
        action = "Import";
    }

    let movieId = $(document.getElementById($("#movieSearch").val().toLowerCase())).text();

    $(formId).attr("action", `/Movies/${action}/` + movieId);
    console.log(movieId);
}

$(document).ready(function () {
    // Add smooth scrolling to all links
    $("a").on('click', function (event) {

        // Make sure this.hash has a value before overriding default behavior
        if (this.hash !== "") {
            // Prevent default anchor click behavior
            event.preventDefault();

            // Store hash
            var hash = this.hash;

            // Using jQuery's animate() method to add smooth page scroll
            // The optional number (800) specifies the number of milliseconds it takes to scroll to the specified area
            $('html, body').animate({
                scrollTop: $(hash).offset().top
            }, 800, function () {

                // Add hash (#) to URL when done scrolling (default click behavior)
                window.location.hash = hash;
            });
        } // End if
    });
});