const margin = { top: 70, right: 70, bottom: 70, left: 70 }
let dimensions = [];
function GetDimensions() {
    const parentDiv = document.getElementById("graph");
    const width = parentDiv.clientWidth - margin.top - margin.bottom;
    const height = parentDiv.clientHeight - margin.left - margin.right;

    dimensions[0] = width;
    dimensions[1] = height;
};

function DrawGraph(datasets) {
    /*const width = dimensions[0];
    const height = dimensions[1];*/

    d3.select("#graph").selectAll("*").remove(); //To create new instances of the svg component so that it can be updated with new dimensions


    const svg = d3.select("#graph")
        .append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", `translate(${margin.left}, ${margin.top})`);

    const timestamps = datasets.flatMap(ts => ts.timestamps);
    const x = d3.scaleTime()
        .domain(d3.extent(timestamps, timestamp => new Date(timestamp)))
        .range([0, width]);

    const xAxisCall = d3.axisBottom(x)
        .ticks(d3.timeMonth.every(1))
        .tickFormat(d3.timeFormat("%b %Y"));
    svg.append("g")
        .attr("transform", `translate(0, ${height})`)
        .call(xAxisCall);

    const values = datasets.flatMap(ts => ts.values);
    const y = d3.scaleLinear()
        .domain(d3.extent(values))
        .range([height, 0]);

    const yAxisCall = d3.axisLeft(y);
    svg.append("g")
        .call(yAxisCall);
    
    //need to figure out how to draw multiple line graphs on the same chart
    //A TS object contains List of timestamps and a List of values

    datasets.forEach(ts => {
        const formattedDataset = ts.timestamps.map((timestamp, index) => ({
            timestamp: new Date(timestamp),
            value: ts.values[index]
        }));

        svg.append("path")
            .datum(formattedDataset)
            .attr("fill", "none")
            .attr("stroke", "steelblue")
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => y(datapoint.value)))
    });
};

let hasIntialGraphBeenDrawn = false;
function DynamicDrawGraph(datasets) {
    if (!hasIntialGraphBeenDrawn) {
        DrawGraph(datasets);
        hasIntialGraphBeenDrawn = true;
    } else {
        window.addEventListener("resize", GetDimensions);
        DrawGraph(datasets);
    }
}