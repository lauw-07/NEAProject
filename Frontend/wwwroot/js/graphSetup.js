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
}

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
}

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
}

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
}

export default function SetUpGraph(svg, x, y, height, width, pos, hasPnl) {
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
}
