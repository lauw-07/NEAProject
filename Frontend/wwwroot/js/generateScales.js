export function CreateXScale(timestamps, width) {
    const padding = 20;

    return d3.scaleTime()
        .domain(d3.extent(timestamps))
        .range([padding, width]);
}

export function CreateYScale(values, height) {
    return d3.scaleLinear()
        .domain(d3.extent(values))
        .range([height, 0]);
}