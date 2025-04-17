<script lang="ts">
	import type { Entry } from "./GalleryView.svelte";
	import GalleryView from "./GalleryView.svelte";
	import PageControls from "./PageControls.svelte";

    let { 
        entries,
        totalResults,
        page = $bindable(),
        pageSize,
        maxPage,
    } : { entries: Entry[], totalResults: number, page: number, pageSize: number, maxPage: number } = $props();
</script>

<div id="container">
    <PageControls
        bind:page={page}
        totalResults={totalResults}
        pageSize={pageSize}
        maxPage={maxPage}
    ></PageControls>
    
    <GalleryView entries={entries}></GalleryView>

    <PageControls
        bind:page={page}
        totalResults={totalResults}
        pageSize={pageSize}
        maxPage={maxPage}
    ></PageControls>
</div>

<style>
    #container {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }
</style>