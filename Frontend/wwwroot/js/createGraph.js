const margin = { top: 70, right: 70, bottom: 70, left: 70 }

function GetDimensions(divId) {
    const width = parseInt(d3.select(`#${divId}`).style('width')) - margin.left - margin.right;
    const height = parseInt(d3.select(`#${divId}`).style('height')) - margin.top - margin.bottom;
    return { width, height }
};

function CreateXScale(timestamps, width) {
    const padding = 20;

    return d3.scaleTime()
        .domain(d3.extent(timestamps))
        .range([padding, width]);
};

function CreateYScale(values, height) {
    return d3.scaleLinear()
        .domain(d3.extent(values))
        .range([height, 0]);
};

function CreateYGridlines(svg, x, height) {
    svg.selectAll("verticalGrid")
        .data(x.ticks())
        .join("line")
        .attr("x1", d => x(d))
        .attr("x2", d => x(d))
        .attr("y1", 0)
        .attr("y2", height)
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 0.25)
};

function CreateXGridlines(svg, y, width, padding) {
    svg.selectAll("horizontalGrid")
        .data(y.ticks())
        .join("line")
        .attr("x1", 0)
        .attr("x2", width + padding)
        .attr("y1", d => y(d))
        .attr("y2", d => y(d))
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 0.25)
};

function CreateXAxis(svg, x, height) {
    const xAxis = d3.axisBottom(x)
        .ticks(d3.timeMonth.every(3))
        .tickFormat(d3.timeFormat("%b %Y"));

    svg.append("g")
        .attr("transform", `translate(0, ${height})`)
        .style("font-size", "13px")
        .call(xAxis)
        .call(g => g.select(".domain").remove())
        .selectAll(".tick line")
        .style("stroke-opacity", 0)

    svg.selectAll(".tick text")
        .attr("fill", "#a0a3a1")
};

function CreateYAxis(svg, y, width, pos) {
    const yAxis = (pos === "left") ? d3.axisLeft(y) : d3.axisRight(y);

    svg.append("g")
        .attr("transform", pos === "right" ? `translate(${width}, 0)` : "")
        .style("font-size", "13px")
        .call(yAxis)
        .call(g => g.select(".domain").remove())
        .style("stroke-opacity", 0)
        .selectAll(".tick text")
        .style("fill", "#a0a3a1")
};

function SetUpGraph(svg, x, y, height, width, pos, hasPnl) {
    if (hasPnl) {
        // in case both yPnl and yPriceData are passed in
        CreateYAxis(svg, y[0], width, pos[0]);
        CreateYAxis(svg, y[1], width, pos[1]);
        CreateXGridlines(svg, y[0], width, 20);
    } else {
        CreateYAxis(svg, y[0], width, pos);
        CreateXGridlines(svg, y[0], width, 20);
    }

    CreateXAxis(svg, x, height);
    CreateYGridlines(svg, x, height);
};

function CreateTooltip() {
    return d3.select("body").append("div")
        .attr("class", "tooltip");
};

function UpdateTooltip(tooltip, mouseEvent, content) {
    tooltip.style("display", "block")
        .html(content)
        .style("left", (mouseEvent.pageX + 10) + "px")
        .style("top", (mouseEvent.pageY - 10) + "px");
    // mouseEvent is to keep track of where the mouse is and so let's the function know where to create the tooltip relative to the mouse
};

function HideTooltip(tooltip) {
    tooltip.style("display", "none");
};

function CreateCrosshair(svg) {
    const crosshair = svg.append("g")
        .style("display", "none");

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

    return crosshair;
};

function UpdateCrosshair(event, tooltip, svg, height, width, xScale, dataset) {
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

    //Ts = timestamp
    // example dataset: { "IndicatorTs" : [{ "Timestamps" : [ 1230912, 1230912, 1230912, 1230912 ] }, { "Values" : [ 123, 123, 123, 123 ] } ]}
    const mouseTs = xScale.invert(mouseXcoord);
    let closestValues = {};

    Object.entries(dataset).forEach(([label, data]) => {
        const timestamps = data["timestamps"].map(ts => d3.timeParse("%d/%m/%Y")(ts));
        const values = data["values"];

        const closestIndex = timestamps.reduce((prev, curr, index) => {
            if (Math.abs(curr - mouseTs) < Math.abs(timestamps[prev] - mouseTs)) {
                return index;
            } else {
                return prev;
            }
        }, 0);

        const closestValue = values[closestIndex];
        closestValues[label] = closestValue;
    });

    let toolTipContent = "";

    Object.entries(closestValues).forEach(([label, value]) => {
        toolTipContent += `<div class="tooltip-content">${label}: ${value}</div>`;
    });

    UpdateTooltip(tooltip, event, toolTipContent);
};

function DrawGraph(datasets, divId) {
    const { width, height } = GetDimensions(divId);

    //To create new instances of the svg component so that it can be updated with new dimensions
    d3.select(`#${divId}`).selectAll("*").remove();

    const svg = d3.select(`#${divId}`)
        .append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", `translate(${margin.left}, ${margin.top})`);

    const pnlData = datasets["P&L"] ? datasets["P&L"] : null;
    let priceData = {};

    Object.entries(datasets).forEach(([label, data]) => {
        if (label !== "P&L") {
            priceData[label] = data;
        }
    });

    const timestamps = Object.values(datasets).flatMap(data => data["timestamps"]);
    const formattedTimestamps = timestamps.map(ts => d3.timeParse("%d/%m/%Y")(ts));
    const padding = 20;
    const x = CreateXScale(formattedTimestamps, width);

    const priceDataValues = Object.values(priceData).flatMap(data => data["values"]);
    const yPriceData = CreateYScale(priceDataValues, height);

    let y = [yPriceData];
    let pos = ["left"];

    if (pnlData) {
        const yPnl = CreateYScale(pnlData.values, height);
        y.push(yPnl);
        pos.push("right");

        const formattedPnlData = pnlData["timestamps"].map((ts, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(ts),
            value: pnlData["values"][index]
        }));

        svg.append("path")
            .datum(formattedPnlData)
            .attr("fill", "none")
            .attr("stroke", "red")
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => yPnl(datapoint.value))
            );
    }

    SetUpGraph(svg, x, y, height, width, pos, (pnlData) ? true : false);

    const tooltip = CreateTooltip();
    const crosshair = CreateCrosshair(svg);

    svg.append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("fill", "none")
        .attr("pointer-events", "all")
        .on("mouseover", event => {
            crosshair.style("display", "block");
            UpdateCrosshair(event, tooltip, svg, height, width, x, datasets);
        })
        .on("mousemove", event => UpdateCrosshair(event, tooltip, svg, height, width, x, datasets))
        .on("mouseout", () => {
            crosshair.style("display", "none");
            HideTooltip(tooltip);
        });

    const color = d3.scaleOrdinal(d3.schemeCategory10);

    Object.entries(priceData).forEach(([label, data], i) => {
        const formattedDataset = data["timestamps"].map((ts, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(ts),
            value: data["values"][index]
        }));

        svg.append("path")
            .datum(formattedDataset)
            .attr("fill", "none")
            .attr("stroke", color(i))
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => yPriceData(datapoint.value))
            );
        i++;
    });
};