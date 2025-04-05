<script module lang="ts">
	export type Entry = {
		id: string;
		date: Date;
		tags: string[];
	};
</script>

<script lang="ts">
	let { entries = $bindable() }: { entries: Entry[] } = $props();

	let clientHeights: number[] = $state(new Array(entries.length).fill(0));

	// fucking magic
	// https://dev.to/hungle00/build-a-masonry-layout-pinterest-layout-3glp

	const RowHeight = 10;
	const RowGap = 20;
	const ImageWidth = 300;

	const getDriveThumbnailLink = (fileId: string) => `https://drive.google.com/thumbnail?id=${fileId}`;
	const getDriveImageLink = (fileId: string) => `https://drive.google.com/file/d/${fileId}/view`;

	/**
	 * gets the number of rows that an element with the given size should span
	 * @param h the height of the element in pixels
	 */
	const getGridElementSpan = (h: number): number => {
		const v = h === 0 ? 10 : Math.ceil(h / (RowHeight + RowGap));
		return v;
	};
</script>

<div id="galleryview-container" style="--R: {RowHeight}px; --G: {RowGap}px; --image-width: {ImageWidth}px">
	{#each entries as entry, index (entry.id)}
		<div
            class="item-container"
			bind:clientHeight={clientHeights[index]}
            style:grid-row-end={"span " + getGridElementSpan(clientHeights[index])}
        >
			<p class="tags">
				<span class="date">
					{entry.date.toLocaleDateString()}
				</span>
				{#if entry.tags.length == 1}
					<span class="primary-tag">{entry.tags[0]}</span>
				{:else}
					<span class="primary-tag">{entry.tags[0]}, </span>
					{entry.tags.slice(1).join(", ")}
				{/if}
			</p>

			<a class="imgc" href={getDriveImageLink(entry.id)}>
				<img src={getDriveThumbnailLink(entry.id)} alt="" />
			</a>
		</div>
	{/each}
</div>

<style>
	#galleryview-container {
		display: grid;
		grid-template-columns: repeat(auto-fill, 300px);

		grid-auto-rows: var(--R);
		grid-row-gap: var(--G);

		column-gap: 4px;
        justify-content: center;
	}

	.item-container {
        padding: 1px;
		display: flex;
		flex-direction: column;
		height: min-content;

		gap: 6px;
        background-color: rgb(252, 252, 252);
        border-top-left-radius: 6px;
        border-top-right-radius: 6px;
        box-shadow: 0px -2px 5px 0px black;
	}

	.imgc {
		display: flex;

		& img {
			object-fit: contain;
			width: var(--image-width);
		}
	}

	.tags {
        margin-left: 6px;
        margin-right: 6px;
        margin-top: 4px;
		font-size: 0.8rem;
	}

	.primary-tag {
		font-weight: bold;
	}

	/* 
        breakpoint is aimed to be around where 2 columns would naturally happen
        + a little padding 
    */
	@media (width <= 650px) {
		#galleryview-container {
			grid-template-columns: 1fr 1fr;
		}

		.imgc img {
			width: 100%;
		}
	}
</style>
