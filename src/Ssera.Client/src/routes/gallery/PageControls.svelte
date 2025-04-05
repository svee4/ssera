<script lang="ts">
    let { 
        totalResults,
        page,
        pageSize,
        maxPage, 
        setPage 
    }: { totalResults: number, pageSize: number, page: number, maxPage: number, setPage: (page: number) => void } = $props();
    
    let localPage = $state(page);

    let start = $derived((page - 1) * pageSize + 1);
    
    let tmpEnd = $derived(page * pageSize);
    let end = $derived(tmpEnd > totalResults ? totalResults : tmpEnd);

    const onsubmit = () => {
        setPage(localPage);
        return false;
    }
</script>

<div id="container">
    <form onsubmit={onsubmit}>
        <button
            onclick={() => { localPage--; return true; }}
            title="Previous page"
            aria-label="Previous page"
            disabled={(localPage - 1) < 1}
            style="width: 4ch"
            type="submit"
        >{"<"}</button>

        <input
            type="number" 
            inputmode="numeric"
            bind:value={localPage}
            min="1"
            max={maxPage}
            title="Choose page"
            aria-label="Choose page"
            style="width: 8ch"
        />

        <button
            onclick={() => { localPage++; return true; }}
            title="Next page"
            aria-label="Next page"
            disabled={(localPage + 1) > maxPage}
            style="width: 4ch"
            type="submit"
        >{">"}</button>   
     </form>

     <p>
        Showing {start} - {end} of {totalResults} results
     </p>
</div>

<style>
    #container {
        display: flex;
        justify-content: space-between;
    }
</style>