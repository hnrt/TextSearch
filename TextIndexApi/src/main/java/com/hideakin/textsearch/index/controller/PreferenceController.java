package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.UpdatePreferencesRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.service.PreferenceService;

@RestController
public class PreferenceController {

	@Autowired
	private PreferenceService preferenceService;
	
	@RequestMapping(value="/preference/{name}",method=RequestMethod.GET)
	public ValueResponse getPreference(
			@PathVariable String name) {
		return preferenceService.getPreference(name);
	}

	@RequestMapping(value="/preference",method=RequestMethod.POST)
	public void updatePreferences(
			@RequestBody UpdatePreferencesRequest req) {
		preferenceService.updatePreferences(req);
	}

	@RequestMapping(value="/preference/{name}",method=RequestMethod.DELETE)
	public void deletePreference(
			@PathVariable String name) {
		preferenceService.deletePreference(name);
	}

}
