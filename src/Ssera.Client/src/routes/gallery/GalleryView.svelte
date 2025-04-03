<script module lang="ts">
    export type Entry = {
        id: string;
        date: Date;
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
        <span class="date">
            {entry.date.toLocaleDateString()}&nbsp;
        </span>
        {#if entry.tags.length == 1}
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


        /* 
        
        THE CORRECT COMBINATIONS TO USE HERE ARE VERY DELICATE:
        WHEN SPECIFYING WIDTH: SPECIFY IMG HEIGHT
        WHEN SPECIFYING HEIGHT: SPECIFY IMG WIDTH

        FUCK AROUND AND FIND OUT AT YOUR OWN RISK

        */
        height: 300px;
        /* width: 200px; */

        & a {
            width: 100%;
            height: 100%;

            & img {
                object-fit: contain;
                height: 100%;
                /* width: 100%; */
            }
        }
    }

    @media (width <= 500px) {
        #container {
            display: grid;
            grid-template-columns: 1fr 1fr;
        }

        .item-container {
            width: unset;
            height: unset;

            & a img {
                width: 100%;
                height: unset;
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