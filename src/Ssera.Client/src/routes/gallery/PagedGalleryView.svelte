<script lang="ts">
	import type { Entry } from "./GalleryView.svelte";
	import GalleryView from "./GalleryView.svelte";
	import PageControls from "./PageControls.svelte";

    let { 
        entries,
        totalResults,
        page,
        pageSize,
        maxPage,
        setPage
    } : { entries: Entry[], totalResults: number, page: number, pageSize: number, maxPage: number, setPage: (page: number) => void } = $props();
</script>

<div id="container">
    <PageControls
        totalResults={totalResults}
        page={page}
        pageSize={pageSize}
        maxPage={maxPage}
        setPage={setPage}
    ></PageControls>
    
    <GalleryView entries={entries}></GalleryView>

    <PageControls
        totalResults={totalResults}
        page={page}
        pageSize={pageSize}
        maxPage={maxPage}
        setPage={setPage}
    ></PageControls>
</div>

<style>
    #container {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }
</style>