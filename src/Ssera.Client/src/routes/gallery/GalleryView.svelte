<script module lang="ts">
    export type Entry = {
        id: string;
        tags: string[];
    }
</script>

<script lang="ts">
    let { entries }: { entries: Entry[] } = $props();

    const getDriveThumbnailLink = (fileId: string) => `https://drive.google.com/thumbnail?id=${fileId}`
    const getDriveImageLink = (fileId: string) => `https://drive.google.com/file/d/${fileId}/view`;


</script>

<div id="container">
    {#each entries as entry (entry.id)}
    <div class="item-container">
        <p class="tags">
        {#if entry.tags.length == 0}
            &nsbp;
        {:else if entry.tags.length == 1}
        <span class="primary-tag">{entry.tags[0]}</span>
        {:else}
        <span class="primary-tag">{entry.tags[0]}, </span>
        {entry.tags.slice(1).join(", ")}
        {/if}
        </p>
        <a href={getDriveImageLink(entry.id)}>
            <img src={getDriveThumbnailLink(entry.id)} alt="" />
        </a>
    </div>
    {/each}
</div>

<style>
    #container {
        width: 100%;
        display: flex;
        flex-wrap: wrap;
        justify-content: space-evenly;
        gap: 12px;
    }

    .item-container {
        display: flex;
        flex-direction: column;
        gap: 6px;
        border: 1px solid rgb(252, 252, 252);
        height: min-content;

        & a {
            max-width: 300px;
            max-height: 400px;

            & img {
                object-fit: contain;
                width: 100%;
                height: 100%;
            }
        }
    }

    .tags {
        font-size: 0.8rem;
    }

    .primary-tag {
        font-weight: bold;
    }
</style>