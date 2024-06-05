import { ApiHelper } from "$lib/ApiHelper";

export module HistoryApiHelper {

	export const ApiRoute = `${ApiHelper.ApiDomain}/api/history` as const;

	export type ApiResponse = {
		dateUtc: string,
		totalEvents: number,
		timeTaken: string,
		message?: string
	}[];
}
