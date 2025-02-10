export function CreateTooltip() {
    return d3.select("body").append("div")
        .attr("class", "tooltip");
}

export function UpdateTooltip(tooltip, mouseEvent, content) {
    tooltip.style("display", "block")
        .html(content)
        .style("left", (mouseEvent.pageX + 10) + "px")
        .style("top", (mouseEvent.pageY - 10) + "px");
    // mouseEvent is to keep track of where the mouse is and so let's the function know where to create the tooltip relative to the mouse
}

export function HideTooltip(tooltip) {
    tooltip.style("display", "none");
}