
console.log("Loaded masonry.js");

// Based on:
// https://dev.to/hungle00/build-a-masonry-layout-pinterest-layout-3glp
// We almost can do this without js because we know the size of the images beforehand,
// but the variable amount of tags that can span multiple lines makes this resize stuff necessary.

// these values are duplicated in Results.razor.cs
const RowHeight = 1;
const RowGap = 5;
const ImageWidth = 200;

const resizeElement = (/** @type {Element} */ element) => {
    const v = Math.ceil(element.clientHeight / (RowHeight + RowGap)) + 1;
    element.style["grid-row-end"] = `span ${v}`;
};

const resizeObserver = new ResizeObserver(entries => {
    for (const entry of entries) {
        resizeElement(entry.target);
    }
});

/** @type {Set<Element>} */
const trackedElements = new Set();

window.updateMasonry = () => {
    console.time("updateMasonry");

    const liveElements = new Set(document.querySelectorAll(".masonry-element"));

    console.log("Checking elements:", Array.from(liveElements));

    for (const liveElement of liveElements) {
        if (!trackedElements.has(liveElement)) {
            resizeElement(liveElement);
            trackedElements.add(liveElement);
            resizeObserver.observe(liveElement);
        }
    }

    for (const trackedElement of trackedElements) {
        if (!liveElements.has(trackedElement)) {
            trackedElements.delete(trackedElement);
            resizeObserver.unobserve(trackedElement);
        }
    }

    console.log("Tracked elements:", Array.from(trackedElements));

    console.timeEnd("updateMasonry");
};
