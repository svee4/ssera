<script lang="ts">
	import Page from "../+page.svelte";

    let { 
        totalResults,
        page = $bindable(),
        pageSize,
        maxPage, 
    }: { totalResults: number, pageSize: number, page: number, maxPage: number } = $props();
    
    let start = $derived((page - 1) * pageSize + 1);
    
    let tmpEnd = $derived(page * pageSize);
    let end = $derived(tmpEnd > totalResults ? totalResults : tmpEnd);

    let inputValue = $state(page);

    $effect(() => {
        if (page > maxPage) {
            page = maxPage;
        } else if (page < 1) {
            page = 1;
        }
    });

    const setPage = (value: number) => {
        if (value === page) {
            return;
        }

        if (value > maxPage) {
            page = maxPage;
        } else if (value < 1) {
            page = 1;
        }
        else {
            page = value;
        }

        inputValue = page;
    }
</script>

<div id="container">
    <div>
        <button
            onclick={() => setPage(page - 1)}
            title="Previous page"
            aria-label="Previous page"
            disabled={(page - 1) < 1}
            style="width: 4ch"
            type="submit"
        >{"<"}</button>

        <input
            type="number" 
            inputmode="numeric"
            bind:value={inputValue}
            onblur={() => setPage(inputValue)}
            min="1"
            max={maxPage}
            title="Choose page"
            aria-label="Choose page"
            style="width: 8ch"
        />

        <button
            onclick={() => setPage(page + 1)}
            title="Next page"
            aria-label="Next page"
            disabled={(page + 1) > maxPage}
            style="width: 4ch"
            type="submit"
        >{">"}</button>   
    </div>

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