<script lang="ts">
    // this component is tightly coupled to the list page
    
	export let 
        pageNumber: number,
		dirtyPageNumber: number,
		pageSize: number,
		totalResults: number,
		setPageNumber: (arg0: number) => void;
</script>

<div style="display: flex; justify-content: space-between; align-items: center;">
	<div style="width: 100%; display: flex; width: min-content; height: 100%; gap: 0.125em;">
		<button type="button" style="width: 4ch" on:click={() => setPageNumber(pageNumber - 1)}
			>{"<"}</button
		>
		<input
			style="width: 8ch"
			type="number"
			value={dirtyPageNumber}
			on:change={(e) => setPageNumber(e.currentTarget.valueAsNumber)}
			max={Math.ceil(totalResults / pageSize)}
			min="1"
		/>
		<button type="button" style="width: 4ch" on:click={() => setPageNumber(pageNumber + 1)}
			>{">"}</button
		>
	</div>
	<div>
		Showing {(pageNumber - 1) * pageSize + 1} - {((n) => (n < totalResults ? n : totalResults))(
			pageSize * (pageNumber - 1) + pageSize,
		)} of {totalResults} results
	</div>
</div>
