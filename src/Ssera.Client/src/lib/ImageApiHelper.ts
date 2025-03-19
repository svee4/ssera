import { ApiDomain } from "./const/ApiDomain";

export namespace ImageApiHelper
{
    export const ApiRoute = `${ApiDomain}/api/images`;

    export type GetResponse = {
        results: GetResponseResult[]
    }

    export type GetResponseResult = {
        id: string;
        member: GroupMember;
        tags: Tag[]
        era?: Era;
    }

    export enum OrderByType {
        Date,
        FirstTag
    }

    export enum SortType {
        Ascending,
        Descending
    }

    export enum GroupMember {
        Chaewon,
        Sakura,
        Yunjin,
        Kazuha,
        Eunchae
    }

    export enum Era {
        Fearless,
        Antifragile,
        Unforgiven,
        PerfectNight,
        Easy,
        Crazy,
        Hot
    }

    export type Tag = string;

    export const EraToHuman = (era: Era): string => {
        if (era === Era.PerfectNight) {
            return "Perfect Night";
        } 

        return Era[era];
    }
}