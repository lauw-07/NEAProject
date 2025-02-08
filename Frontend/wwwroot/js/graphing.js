const margin = { top: 70, right: 70, bottom: 70, left: 70 }

function GetDimensions(divId) {
    /*const parentDiv = document.getElementById("graph");
    const width = parentDiv.clientWidth - margin.top - margin.bottom;
    const height = parentDiv.clientHeight - margin.left - margin.right;

    dimensions[0] = width;
    dimensions[1] = height;*/

    const width = parseInt(d3.select(`#${divId}`).style('width')) - margin.top - margin.bottom;
    const height = parseInt(d3.select(`#${divId}`).style('height')) - margin.left - margin.right;

    return { width, height }
};

function CreateTooltip() {
    return d3.select("body").append("div")
        .attr("class", "tooltip");
}

function UpdateTooltip(tooltip, mouseEvent, content) {
    tooltip.style("display", "block")
        .html(content)
        .style("left", (mouseEvent.pageX + 10) + "px")
        .style("top", (mouseEvent.pageY - 10) + "px");

    // mouseEvent is to keep track of where the mouse is and so let's the function know where to create the tooltip relative to the mouse
}

function HideTooltip(tooltip) {
    tooltip.style("display", "none");
}

function DrawGraph(datasets, divId) {
    /*const width = dimensions[0];
    const height = dimensions[1];*/
    const { width, height } = GetDimensions(divId);

    if (!Array.isArray(datasets)) {
        datasets = [datasets];  
    }
    
    //To create new instances of the svg component so that it can be updated with new dimensions
    d3.select(`#${divId}`).selectAll("*").remove();
    
    const svg = d3.select(`#${divId}`)
        .append("svg")
        //.attr("preserveAspectRatio", "xMinYMin meet")
        //.attr("viewBox", `0 0 ${width} ${height}`)
        //.attr("viewBox", `0 0 ${width + margin.left + margin.right} ${height + margin.top + margin.bottom}`)
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", `translate(${margin.left}, ${margin.top})`);

    const padding = 20;
    const timestamps = datasets.flatMap(ts => ts.timestamps);
    const x = d3.scaleTime()
        .domain(d3.extent(timestamps, timestamp => d3.timeParse("%d/%m/%Y")(timestamp)))
        .range([padding, width]);

    const xAxisCall = d3.axisBottom(x)
        .ticks(d3.timeMonth.every(3))
        .tickFormat(d3.timeFormat("%b %Y"));

    svg.append("g")
        .attr("transform", `translate(0, ${height})`)
        .style("font-size", "13px")
        .call(xAxisCall)
        .call(g => g.select(".domain").remove())
        .selectAll(".tick line")
        .style("stroke-opacity", 0)

    svg.selectAll(".tick text")
        .attr("fill", "#a0a3a1")
        


    const values = datasets.flatMap(ts => ts.values);
    const y = d3.scaleLinear()
        .domain(d3.extent(values))
        .range([height, 0]);

    const yAxisCall = d3.axisLeft(y);
    svg.append("g")
        .style("font-size", "13px")
        .call(yAxisCall)
        .call(g => g.select(".domain").remove())
        .style("stroke-opacity", 0)
        .selectAll(".tick text")
        .style("fill", "#a0a3a1")

    svg.selectAll("verticalGrid")
        .data(x.ticks())
        .join("line")
        .attr("x1", d => x(d))
        .attr("x2", d => x(d))
        .attr("y1", 0)
        .attr("y2", height)
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 0.25)

    svg.selectAll("horizontalGrid")
        .data(y.ticks())
        .join("line")
        .attr("x1", 0)
        .attr("x2", width + padding)
        .attr("y1", d => y(d))
        .attr("y2", d => y(d))
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 0.25)


    const tooltip = CreateTooltip();

    const crosshair = svg.append("g")
        .style("display", "none")

    crosshair.append("line")
        .attr("id", "crosshairXdir")
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 1.5)
        .attr("stroke-opacity", 2)
        .attr("stroke-dasharray", "2,2")

    crosshair.append("line")
        .attr("id", "crosshairYdir")
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 1.5)
        .attr("stroke-opacity", 2)
        .attr("stroke-dasharray", "2,2")

    svg.append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("fill", "none")
        .attr("pointer-events", "all")
        .on("mouseover", event => {
            crosshair.style("display", null);
            UpdateCrosshair(event, tooltip)
        })
        .on("mousemove", event => UpdateCrosshair(event, tooltip))
        .on("mouseout", () => {
            crosshair.style("display", null);
        });

    function UpdateCrosshair(event, tooltip) {
        const [mouseXcoord, mouseYcoord] = d3.pointer(event, svg.node());

        d3.select("#crosshairXdir")
            .attr("x1", mouseXcoord)
            .attr("x2", mouseXcoord)
            .attr("y1", 0)
            .attr("y2", height);

        d3.select("#crosshairYdir")
            .attr("x1", 0)
            .attr("x2", width)
            .attr("y1", mouseYcoord)
            .attr("y2", mouseYcoord);

        UpdateTooltip(tooltip, event, "");
    }

    //need to figure out how to draw multiple line graphs on the same chart
    //A TS object contains List of timestamps and a List of values
    const color = d3.scaleOrdinal(d3.schemeCategory10);
    let i = 0;
    datasets.forEach(ts => {
        const formattedDataset = ts.timestamps.map((timestamp, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(timestamp),
            value: ts.values[index]
        }));

        svg.append("path")
            .datum(formattedDataset)
            .attr("fill", "none")
            .attr("stroke", color(i))
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => y(datapoint.value))
            );
        i++;
    });
};